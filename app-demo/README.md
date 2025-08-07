# Performance API Demo

Esta é uma API de demonstração construída com .NET 8 e C# que expõe um único endpoint para obter extratos de uma conta. A API utiliza, inicialmente, o Entity Framework para se conectar a um banco de dados Microsoft SQL Server.

## Otimizações
- O arquivo [IMPROVEMENTS_STEP_BY_STEP.md](IMPROVEMENTS_STEP_BY_STEP.md) contém as instruções para otimização da API, etapa por etapa (como foi demonstrado na apresentação ao vivo).
- O arquivo [TIME_MEASURE_LOCAL.md](TIME_MEASURE_LOCAL.md) contém métricas extraídas da execução do teste de carga num ambiente local.

## Pré-requisitos para execução

Para executar esta aplicação, você precisará ter os seguintes softwares instalados:

*   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   [Microsoft SQL Server](https://www.microsoft.com/pt-br/sql-server/sql-server-downloads) (ou uma instância do SQL Server em execução no Docker)

## Como Executar

1.  **Clone o repositório:**

    ```bash
    git clone <URL_DO_REPOSITORIO>
    cd app-demo
    ```

2.  **Configure a conexão com o banco de dados:**

    Abra o arquivo `PerformanceApi/appsettings.json` e edite a `ConnectionString` "DefaultConnection" para apontar para a sua instância do SQL Server.

    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=SEU_SERVIDOR;Database=performance-demo-db;User ID=SEU_USUARIO;Password=SUA_SENHA;TrustServerCertificate=True;"
      },
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      },
      "AllowedHosts": "*"
    }
    ```

3.  **Execute a API:**

    Navegue até o diretório da API e execute o seguinte comando:

    ```bash
    cd PerformanceApi
    dotnet run
    ```

    A API estará em execução em `http://localhost:5227` (ou em outra porta, verifique o output do console e o arquivo de configuração em `~\PerformanceApi\Properties\launchSettings.json`).

## Endpoint

A API expõe o seguinte endpoint:

### Obter Extrato da Conta

*   **GET** `/api/Statement/{accountId}`

Recupera o extrato de uma conta para um determinado período.

**Parâmetros:**

*   `accountId` (obrigatório): O ID da conta.
*   `startDate` (opcional): A data de início do extrato (formato: `YYYY-MM-DD`). Se não for fornecido, o padrão é um mês antes da data de término.
*   `endDate` (opcional): A data de término do extrato (formato: `YYYY-MM-DD`). Se não for fornecido, o padrão é a data atual.

**Exemplo de Requisição:**

```bash
curl -X GET "http://localhost:5000/api/Statement/1?startDate=2024-01-01&endDate=2024-01-31"
```

**Exemplo de Resposta:**

```json
{  
  "transactions": [
    {
      "amount": 100.00,
      "categoryName": "SUPERMERCADO",
      "description": "Compra no supermercado",
      "date": "2024-01-15T14:30:00Z",
      "riskScore": 0
    },
    {
      "amount": 100.00,
      "categoryName": "CARTÃO",
      "description": "Pagamento da fatura",
      "date": "2024-01-20T10:00:00Z",
      "riskScore": 3
    }
  ]
}
```
