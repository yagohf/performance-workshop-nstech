## Passo 1: Executar a carga AS-IS
Primeiro, vamos observar latência altíssima e RPS muito baixo;

Em seguida, temos que analisar o APM (Jaeger) para identificarmos queries lentas e em grande número (N+1);

## Passo 2: Desativar o lazy loading e reenviar a carga

Precisamos desligar o Lazy Loading direto na configuração do DbContext:

### Program.cs
```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PerformanceDbContext>(options =>
    // options.UseLazyLoadingProxies()
    .UseSqlServer(connectionString));
```

Em seguida, precisamos mudar a query para que já seja feito o .Include() dos dados da categoria:

### StatementRepository.cs
```csharp
var transactions = await _context.Transactions
            .AsNoTracking() // Desligar o lazy loading para essa query.
            .Include(t=> t.Category) // Já carregar automaticamente a categoria.
            .Where(t => t.AccountId == accountId && t.TransactionDate >= startDate && t.TransactionDate <= endDate)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
```

É interessante observar o aumento de RPS, certa redução de latência, mas números ainda bastante ruins;

## Passo 3: Vamos rodar a análise da query via Rider (Explain Plan)
Observar que o custo da query é alto e está concentrado nas operações de full scan e na ordenação por data;

Em seguida, vamos criar um índice compreensivo para essa query, incluindo todos os campos que utilizamos na consulta;

```sql
CREATE NONCLUSTERED INDEX IX_Transactions_AccountId_TransactionDate
    ON dbo.Transactions (AccountId, TransactionDate DESC)
    INCLUDE (Id, Amount, CategoryId, Description);
```

Agora, reexecutaremos a carga e observaremos o aumento de RPS e redução de latência, mas ainda estamos longe do ideal;

## Passo 4: Profiling do alto consumo de memória
Vamos rodar o dotnet-counters e verificar a taxa de alocação de memória por segundo, o uso recorrente do GC e o aumento da Gen0 da Heap;

### Listar processos que podem ser monitorados pelo dotnet-counters
```bash
dotnet-counters ps
```

### Executar o monitoramento ao vivo do processo
```bash
dotnet-counters monitor -p <PID>
```

Vamos rodar em seguida o dotMemory, e vamos inspecionar as gerações do GC para entendimento de quais são os tipos de objetos sendo alocados excessivamente;

Aqui deve ficar evidente a quantidade de "tranqueiras" alocadas pelo EF, então vamos trocar de ORM: vamos do EF para o Dapper;

### IDbConnectionFactory.cs e SqlServerConnectionFactory.cs
```csharp
public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

public class SqlServerConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlServerConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
```

### StatementDapperRepository.cs
```csharp
public class StatementDapperRepository : IStatementRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public StatementDapperRepository(IDbConnectionFactory dbConnectionFactoryFactory)
    {
        _dbConnectionFactory = dbConnectionFactoryFactory;
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByAccountAsync(int accountId, DateTime startDate,
        DateTime endDate)
    {
        const string query = @"
            SELECT 
                t.Id, 
                t.AccountId, 
                t.TransactionDate, 
                t.Amount, 
                t.Description, 
                t.CategoryId, 
                c.Id,
                c.Name
            FROM Transactions t
            INNER JOIN Categories c ON t.CategoryId = c.Id
            WHERE t.AccountId = @AccountId
                AND t.TransactionDate >= @StartDate
                AND t.TransactionDate <= @EndDate
            ORDER BY t.TransactionDate DESC
        ";

        using var conn = _dbConnectionFactory.CreateConnection();
        var transactions = await conn.QueryAsync<Transaction, Category, Transaction>(
            query,
            (transaction, category) =>
            {
                transaction.Category = category; // Realiza o mapeamento da categoria para a transação
                return transaction;
            },
            new
            {
                AccountId = accountId, 
                StartDate = startDate, 
                EndDate = endDate
            },
            splitOn: "CategoryId"
        );
        
        return transactions;
    }
}
```

### Program.cs
```csharp
// 2.2. Dapper
builder.Services.AddSingleton<IDbConnectionFactory>(
    new SqlServerConnectionFactory(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IStatementRepository, StatementDapperRepository>(); 
```

Em seguida, nova execução da carga, para verificarmos que o RPS praticamente dobrou, a memória alocada caiu pela metade, e a latência também. Mas ainda há espaço para melhoria;

## Passo 5: Verificar a quantidade de dados retornados pela query
Vamos rodar um comando count(*) para verificarmos que a quantidade de dados retornados por cada query é consideravelmente grande (cerca de 2000 registros);
```sql
SELECT COUNT(*)
FROM Transactions t
         INNER JOIN Categories c ON t.CategoryId = c.Id
WHERE t.AccountId = 1
  AND t.TransactionDate >= '2025-05-07'
  AND t.TransactionDate <= '2025-08-07';
```

Aqui precisamos rodar uma conta básica para estimarmos o tamanho da massa de dados sendo trafegada: campos da tabela x bytes usados para cada campo x número de resultados;
```text
Resultado do explain plain:
- AvgRowSize: 300 bytes;
- Nro de linhas retornadas por query: 2000 (em média)
- Total de bytes trafegados via rede por query: 300 x 2000 = 600.000 bytes (ou 600 Kb)
- 50 clientes simultâneos = tráfego de 30MB/segundo pela rede e alocação de 30MB/segundo (pelo menos) em nossa aplicação
```

Nunca teremos uma performance aceitável trafegando tantos dados. A alocação ainda continua bem alta, o GC ainda está trabalhando muito; 

Então, é hora de paginarmos os resultados, limitando a 50 registros por página:
```sql
SELECT 
    t.Id, 
    t.AccountId, 
    t.TransactionDate, 
    t.Amount, 
    t.Description, 
    t.CategoryId, 
    c.Id,
    c.Name
FROM Transactions t
INNER JOIN Categories c ON t.CategoryId = c.Id
WHERE t.AccountId = @AccountId
    AND t.TransactionDate >= @StartDate
    AND t.TransactionDate <= @EndDate
ORDER BY t.TransactionDate DESC
OFFSET @Offset ROWS 
FETCH NEXT @PageSize ROWS ONLY
```

```csharp
new
{
    AccountId = accountId, 
    StartDate = startDate, 
    EndDate = endDate,
    Offset = offset,
    PageSize = pageSize
}
```

Vamos reexecutar a carga e notaremos que o RPS chegou a valores imensamente maiores do que o que tínhamos no começo;

## Passo 6: O time de produto pediu um cálculo de score de risco por transação
Vamos incluir o processamento ineficiente para calcular o risco usando um algoritmo O(n²);

Antes de avançarmos, para ilustrar, vamos aumentar o tamanho das páginas de dados para 200 itens por página;

Precisamos também desativar os monitoramentos para evitar interferências na demo;

Vamos rodar uma nova carga e poderemos verificar que o RPS caiu, pois aumentamos coisas a serem processadas;

Vamos rodar o dotTrace no modo "sampling", visualizando que boa parte do tempo de processamento está sendo gasto no método de cálculo de risco;

Aqui cabe uma breve explicação sobre Big(O) e também uma recomendação de usar IA para analisar a complexidade de tempo do seu algoritmo;

Se temos 200 itens por página e cada execução é de ordem O(n²), teremos 200x200 = 40.000 iterações por execução;

## Passo 7: Otimização simples do cálculo de score de risco por transação
Vamos introduzir a versão que agrupa os lançamentos por data, usando ILookup<T>;

### StatementService.cs
```csharp
var transactionsByDate = transactions.ToLookup(t => t.TransactionDate.Date);
```

### RiskScoreService.cs (e IRiskScoreService.cs)
```csharp
decimal ComputeRiskScore(Transaction currentTransaction, ILookup<DateTime, Transaction> transactionsByDate);

public decimal ComputeRiskScore(Transaction currentTransaction, ILookup<DateTime, Transaction> transactionsByDate)
{
    var sameDayTransactions = transactionsByDate[currentTransaction.TransactionDate.Date];
    decimal riskScore = 0m;
    
    foreach (var other in sameDayTransactions)
    {
        if (other.Id != currentTransaction.Id && Math.Abs(other.Amount - currentTransaction.Amount) <= 1.0m)
        {
            riskScore++;
        }
    }

    return riskScore;
}
```

A complexidade do algoritmo agora passa a ser O(n), mas são duas iterações de O(n): uma para agrupar os dados, outra para consultar as transações do dia e calcular o score para cada item da lista;