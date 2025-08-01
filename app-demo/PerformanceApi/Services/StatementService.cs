using System.Collections;
using PerformanceApi.Models.Dto;
using PerformanceApi.Models.Entity;
using PerformanceApi.Repositories;

namespace PerformanceApi.Services;

public class StatementService : IStatementService
{
    private readonly IStatementRepository _repository;

    public StatementService(IStatementRepository repository)
    {
        _repository = repository;
    }

    public async Task<StatementResponseDto> GetStatementAsync(int accountId, DateTime startDate, DateTime endDate)
    {
        //var transactions = await _repository.GetTransactionsByAccountAsync(accountId, startDate, endDate);
        await Task.Delay(TimeSpan.FromMilliseconds(10));
        var transactions = MockTransactions();
        var response = new StatementResponseDto();

        // foreach (var transaction in transactions)
        // {
        //     // #######################################################################
        //     // ### PROBLEMA DE ALTA ALOCAÇÃO (GC PRESSURE) E PICO DE CPU           ###
        //     // #######################################################################
        //     // 1. ToBase64String: Aloca uma nova string para cada protocolo.
        //     // 2. ToUpper: Acessar .Category.Name dispara a query N+1. Em seguida, .ToUpper() cria OUTRA string em maiúsculas.
        //     // 3. string.Format: Aloca uma terceira string para juntar tudo.
        //     // Para 1000 transações, teremos no mínimo 3000 alocações de string desnecessárias.
        //     // O Garbage Collector (GC) trabalhará intensamente para limpar essa memória, causando picos de CPU.
        //     // var formattedDetails = string.Format(
        //     //     "Protocolo: {0} | Categoria: {1}",
        //     //     Convert.ToBase64String(transaction.Protocol),
        //     //     transaction.Category.Name.ToUpperInvariant()); // O acesso a .Category.Name dispara a query N+1
        //
        //     var transactionDto = new TransactionDto
        //     {
        //         Amount = transaction.Amount,
        //         Date = transaction.TransactionDate,
        //         Description = transaction.Description,
        //         CategoryName = transaction.Category.Name, // Outro acesso que dispara a query N+1
        //         // FormattedDetails = formattedDetails
        //     };
        //
        //     response.Transactions.Add(transactionDto);
        // }

        // #######################################################################
        // ### PROBLEMA DE COMPLEXIDADE CICLOMÁTICA - Big O(n²)                ###
        // #######################################################################
        // Simula uma verificação de "risco" completamente ineficiente,
        // comparando cada transação com todas as outras.
        // Se a lista tiver 100 transações, este trecho fará 100*100 = 10.000 iterações.
        // Um prato cheio para o dotTrace mostrar um "hotspot" de CPU.

        decimal balance = 0m;
        // foreach (var transaction in response.Transactions)
        // {
        //     var potentialRiskScore = 0;
        //     foreach (var otherTransaction in response.Transactions)
        //     {
        //         if (transaction.Date == otherTransaction.Date && transaction.Amount > 100)
        //         {
        //             potentialRiskScore++;
        //         }
        //     }
        //     
        //     balance += transaction.Amount;
        // }

        response.Balance = balance;
        return response;
    }

    private IEnumerable<Models.Entity.Transaction> MockTransactions()
    {
        return new List<Models.Entity.Transaction>
        {
            new Models.Entity.Transaction
            {
                Id = 1,
                AccountId = 101,
                CategoryId = 1,
                TransactionDate = new DateTime(2023, 1, 15),
                Amount = 150.75m,
                Description = "Grocery purchase",
                Protocol = new byte[] { 1, 2, 3 },
                Category = new Models.Entity.Category { Id = 1, Name = "Groceries", Code = "GROC" }
            },
            new Models.Entity.Transaction
            {
                Id = 2,
                AccountId = 101,
                CategoryId = 2,
                TransactionDate = new DateTime(2023, 1, 16),
                Amount = 200.50m,
                Description = "Electronics purchase",
                Protocol = new byte[] { 4, 5, 6 },
                Category = new Models.Entity.Category { Id = 2, Name = "Electronics", Code = "ELEC" }
            },
            new Models.Entity.Transaction
            {
                Id = 3,
                AccountId = 102,
                CategoryId = 3,
                TransactionDate = new DateTime(2023, 1, 17),
                Amount = 75.00m,
                Description = "Fuel refill",
                Protocol = new byte[] { 7, 8, 9 },
                Category = new Models.Entity.Category { Id = 3, Name = "Fuel", Code = "FUEL" }
            },
            new Models.Entity.Transaction
            {
                Id = 4,
                AccountId = 102,
                CategoryId = 1,
                TransactionDate = new DateTime(2023, 1, 18),
                Amount = 120.25m,
                Description = "Supermarket shopping",
                Protocol = new byte[] { 10, 11, 12 },
                Category = new Models.Entity.Category { Id = 1, Name = "Groceries", Code = "GROC" }
            },
            new Models.Entity.Transaction
            {
                Id = 5,
                AccountId = 103,
                CategoryId = 4,
                TransactionDate = new DateTime(2023, 1, 19),
                Amount = 50.00m,
                Description = "Dining out",
                Protocol = new byte[] { 13, 14, 15 },
                Category = new Models.Entity.Category { Id = 4, Name = "Restaurants", Code = "REST" }
            },
            new Models.Entity.Transaction
            {
                Id = 6,
                AccountId = 103,
                CategoryId = 5,
                TransactionDate = new DateTime(2023, 1, 20),
                Amount = 300.90m,
                Description = "Home appliances",
                Protocol = new byte[] { 16, 17, 18 },
                Category = new Models.Entity.Category { Id = 5, Name = "Home", Code = "HOME" }
            },
            new Models.Entity.Transaction
            {
                Id = 7,
                AccountId = 104,
                CategoryId = 3,
                TransactionDate = new DateTime(2023, 1, 21),
                Amount = 90.00m,
                Description = "Car maintenance",
                Protocol = new byte[] { 19, 20, 21 },
                Category = new Models.Entity.Category { Id = 3, Name = "Fuel", Code = "FUEL" }
            },
            new Models.Entity.Transaction
            {
                Id = 8,
                AccountId = 104,
                CategoryId = 6,
                TransactionDate = new DateTime(2023, 1, 22),
                Amount = 45.99m,
                Description = "Clothing purchase",
                Protocol = new byte[] { 22, 23, 24 },
                Category = new Models.Entity.Category { Id = 6, Name = "Clothing", Code = "CLOT" }
            },
            new Models.Entity.Transaction
            {
                Id = 9,
                AccountId = 105,
                CategoryId = 2,
                TransactionDate = new DateTime(2023, 1, 23),
                Amount = 110.40m,
                Description = "Laptop accessories",
                Protocol = new byte[] { 25, 26, 27 },
                Category = new Models.Entity.Category { Id = 2, Name = "Electronics", Code = "ELEC" }
            },
            new Models.Entity.Transaction
            {
                Id = 10,
                AccountId = 105,
                CategoryId = 4,
                TransactionDate = new DateTime(2023, 1, 24),
                Amount = 60.75m,
                Description = "Dinner at restaurant",
                Protocol = new byte[] { 28, 29, 30 },
                Category = new Models.Entity.Category { Id = 4, Name = "Restaurants", Code = "REST" }
            }
        };
    }
}