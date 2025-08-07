# Guia Passo a Passo para Otimização de Performance

Este documento descreve uma série de melhorias de performance aplicadas ao serviço "PerformanceAPI", conforme visto na Demo realizada na apresentação ao vivo. Começaremos com uma versão lenta e ineficiente e aplicaremos otimizações progressivamente, medindo o impacto de cada mudança com várias ferramentas de análise de performance.

## Passo 1: Estabelecendo uma Linha de Base (Baseline)

O passo inicial é executar um teste de carga na aplicação existente (estado `AS-IS`) para estabelecer uma linha de base para os principais indicadores de performance.

### Análise

* **Observação Inicial:** A aplicação exibe **alta latência** e um número muito baixo de **requisições por segundo (RPS)**.
* **Análise de APM (Jaeger):** Uma análise inicial usando uma ferramenta de Application Performance Management (APM) como o Jaeger, revela queries de banco de dados lentas e um número significativo delas, apontando para um potencial problema de **N+1 queries**.

***

## Passo 2: Lidando com o Carregamento ineficiente de dados

O problema de N+1 é frequentemente causado pelo *lazy loading*, onde entidades relacionadas são buscadas do banco de dados em queries separadas para cada entidade pai.

### Solução

1.  **Desativar o Lazy Loading:** A primeira mudança é desativar o *lazy loading* na configuração do `DbContext`.

    *Program.cs*
    ```csharp
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<PerformanceDbContext>(options =>
        // options.UseLazyLoadingProxies() // Lazy loading desativado
        .UseSqlServer(connectionString));
    ```

2.  **Carregamento Ansioso (Eager Loading) com `.Include()`:** Modificamos a query para carregar explicitamente os dados relacionados necessários (`Category`) em uma única consulta usando o método `.Include()`. O uso de `.AsNoTracking()` também é uma boa prática para queries de leitura, pois instrui o Entity Framework Core a não rastrear as entidades retornadas, o que pode reduzir o consumo de memória.

    *StatementRepository.cs*
    ```csharp
    var transactions = await _context.Transactions
                .AsNoTracking() // Desativa o rastreamento de mudanças para ganhos de performance em cenários de apenas leitura.
                .Include(t => t.Category) // Carrega os dados da categoria relacionada de forma ansiosa.
                .Where(t => t.AccountId == accountId && t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
    ```

### Resultado

Após executar o teste de carga novamente, observamos um **aumento no RPS** e uma **redução na latência**. No entanto, a performance ainda não está ideal.

***

## Passo 3: Otimizando as queries de Banco de Dados

Mesmo com o *eager loading*, a própria query pode ser ineficiente.

### Análise

* **Análise do Plano de Execução:** Usando a funcionalidade "Explain Plan" em uma ferramenta de banco de dados, como a que temos disponível no JetBrains Rider, podemos analisar o plano de execução da query. A análise mostra que o **custo da query é alto**, com tempo significativo sendo gasto em um *full table scan* na tabela `dbo.Transactions` e na operação de ordenação. Essa é a query originalmente executada pelo EF:

```sql
SELECT [t].[Id], [t].[AccountId], [t].[Amount], [t].[CategoryId], [t].[Description], [t].[TransactionDate]
FROM [Transactions] AS [t]
WHERE [t].[AccountId] = 1 AND [t].[TransactionDate] >= '2025-05-07' AND [t].[TransactionDate] <= '2025-08-07';
```

**Explain plan:**
![Rider Explain Plan](.\images\01_rider_explain_plan.png)
![Rider Plan Explained](.\images\02_rider_exec_plain_explained.png)


### Solução

* **Criar um Índice Abrangente:** Para resolver isso, criaremos um índice não clusterizado que cobre todos os campos usados nas cláusulas `WHERE` e `ORDER BY` da query. A cláusula `INCLUDE` adiciona as outras colunas selecionadas ao nível folha do índice, o que pode melhorar ainda mais a performance, evitando buscas na tabela principal.

    ```sql
    CREATE NONCLUSTERED INDEX IX_Transactions_AccountId_TransactionDate
        ON dbo.Transactions (AccountId, TransactionDate DESC)
        INCLUDE (Id, Amount, CategoryId, Description);
    ```

### Resultado

Outro teste de carga confirma um **aumento significativo no RPS** e uma **redução adicional na latência**. Embora seja uma melhoria notável, ainda há espaço para otimização.

**Novo plano de execução:**
![Rider new Plan Explained](.\images\03_rider_new_plain_explained.png)

***

## Passo 4: Investigando o consumo de memória

A alta alocação de memória pode levar a ciclos frequentes de *garbage collection* (GC), que podem pausar a aplicação e impactar a performance.

### Análise

* **Profiling de Memória com `dotnet-counters`:** Podemos usar a ferramenta `dotnet-counters` para monitorar contadores de performance em tempo real. Para mais informações, consulte a [documentação do dotnet-counters](https://docs.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-counters).
    * **Listar Processos Monitoráveis:** `dotnet-counters ps`
    * **Monitoramento em Tempo Real:** `dotnet-counters monitor -p <PID>`
    Isso revela uma **alta taxa de alocação de memória** por segundo e coletas de lixo frequentes na Geração 0 (Gen0) do *heap*.

* **Análise Profunda de Memória com dotMemory:** Para entender quais objetos estão sendo alocados excessivamente, podemos usar um *profiler* de memória como o [dotMemory](https://www.jetbrains.com/dotmemory/documentation/) da JetBrains. A análise aponta para um grande número de objetos sendo alocados pelo *Object-Relational Mapper* (ORM), o Entity Framework (EF).

### Solução

* **Mudar para um Micro-ORM (Dapper):** Para reduzir o consumo de memória, substituiremos o EF, que é rico em funcionalidades, por um "micro-ORM" mais enxuto como o Dapper. O Dapper é um mapeador de objetos simples para .NET e tem um consumo de memória menor.

    *IDbConnectionFactory.cs e SqlServerConnectionFactory.cs*
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

    *StatementDapperRepository.cs*
    ```csharp
    public class StatementDapperRepository : IStatementRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public StatementDapperRepository(IDbConnectionFactory dbConnectionFactoryFactory)
        {
            _dbConnectionFactory = dbConnectionFactoryFactory;
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByAccountAsync(int accountId, DateTime startDate, DateTime endDate)
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
                    transaction.Category = category; // Mapeia manualmente a categoria para a transação
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

    *Program.cs*
    ```csharp
    // Registro do Dapper
    builder.Services.AddSingleton<IDbConnectionFactory>(
        new SqlServerConnectionFactory(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddScoped<IStatementRepository, StatementDapperRepository>();
    ```

### Resultado

Um novo teste de carga mostra que o **RPS praticamente dobrou**, e tanto a **alocação de memória quanto a latência foram reduzidas pela metade**.

***

## Passo 5: Reduzindo a transferência de dados

Mesmo com uma query eficiente, retornar um grande volume de dados pode ser um gargalo.

### Análise

* **Avaliação do Volume de Dados:** Uma simples query `COUNT(*)` revela que cada requisição retorna um número substancial de registros (cerca de 2000).

    ```sql
    SELECT COUNT(*)
    FROM Transactions t
             INNER JOIN Categories c ON t.CategoryId = c.Id
    WHERE t.AccountId = 1
      AND t.TransactionDate >= '2025-05-07'
      AND t.TransactionDate <= '2025-08-07';
    ```

* **Estimativa de Tráfego:**
    * Tamanho Médio da Linha: ~300 bytes
    * Linhas por query: ~2000
    * Total de bytes por query: 300 \* 2000 = **600 KB**
    * Com 50 clientes simultâneos, isso resulta em aproximadamente **30 MB/s** de tráfego de rede e alocação de memória.

### Solução

* **Implementar Paginação:** Vamos paginar os resultados, limitando o número de registros retornados por requisição a um tamanho mais gerenciável (ex: 50).

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

    Os parâmetros passados para a query são atualizados para incluir `Offset` e `PageSize`.

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

### Resultado

Ao reexecutar o teste de carga, o **RPS atinge valores imensamente maiores** do que no início de nossas otimizações.

***

## Passo 6: Analisando Operações Intensivas de CPU

Um novo requisito de funcionalidade exige um cálculo de pontuação de risco para cada transação, o que introduz um novo desafio de performance.

### Análise

* **Implementação Inicial:** A implementação inicial do cálculo da pontuação de risco usa um algoritmo ineficiente com uma complexidade de tempo de $O(n²)$.
* **Impacto na Performance:** Após aumentar o tamanho da página para 200 para fins de demonstração, um novo teste de carga mostra uma **queda no RPS** devido ao aumento do tempo de processamento.
* **Profiling de CPU com dotTrace:** Para identificar a causa da lentidão, usamos um *profiler* de CPU como o [dotTrace](https://www.jetbrains.com/dottrace/documentation/) da JetBrains no modo "sampling". Os resultados indicam claramente que uma parte significativa do tempo de processamento é gasta no método de cálculo da pontuação de risco.
* **Notação Big O:** A complexidade de um algoritmo descreve como seu tempo de execução escala com o tamanho da entrada ($n$). Um algoritmo $O(n²)$ tem um tempo de execução que cresce quadraticamente; para 200 itens, ele realizaria aproximadamente 40.000 iterações. [Mais sobre Big O Notation](https://www.bigocheatsheet.com/)

***

## Passo 7: Otimizando a complexidade algorítmica

A chave para melhorar a performance do cálculo da pontuação de risco é reduzir sua complexidade algorítmica.

### Solução

* **Usando `ILookup<T>` para Agrupamento Eficiente:** Podemos otimizar o cálculo agrupando primeiro as transações por data usando `ToLookup()`. Isso permite o acesso rápido a todas as transações de um determinado dia, já que a busca em um `ILookup<T>` é sempre **O(1)**.

    *StatementService.cs*
    ```csharp
    var transactionsByDate = transactions.ToLookup(t => t.TransactionDate.Date);
    ```

* **Cálculo Otimizado da Pontuação de Risco:** O método `ComputeRiskScore` é atualizado para usar esse agrupamento.

    *RiskScoreService.cs*
    ```csharp
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

### Resultado

A complexidade do algoritmo agora é efetivamente **$O(n)$**, pois envolve uma passagem para criar o *lookup* e outra para iterar sobre as transações de um dia específico. Isso leva a uma melhoria substancial na performance e redução do footprint de CPU nos nossos traces, algo bem visível em ferramentas como o dotTrace.