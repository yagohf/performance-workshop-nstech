using Dapper;
using PerformanceApi.Data;
using PerformanceApi.Models.Entity;

namespace PerformanceApi.Repositories
{
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
                    t.CategoryId
                FROM Transactions t
                INNER JOIN Categories c ON t.CategoryId = c.Id
                WHERE t.AccountId = @AccountId
                  AND t.TransactionDate >= @StartDate
                  AND t.TransactionDate <= @EndDate
                ORDER BY t.TransactionDate DESC";

            using var conn = _dbConnectionFactory.CreateConnection();
            // var transactions = await conn.QueryAsync<Transaction, Category, Transaction>(
            //     query,
            //     (transaction, category) =>
            //     {
            //         transaction.Category = category; // Realiza o mapeamento da categoria para a transação
            //         return transaction;
            //     },
            //     new { AccountId = accountId, StartDate = startDate, EndDate = endDate },
            //     splitOn: "CategoryId"
            // );
            
            var transactions = await conn.QueryAsync<Transaction>(query, new { AccountId = accountId, StartDate = startDate, EndDate = endDate });

            //return transactions.ToList();
            return transactions;
        }
    }
}