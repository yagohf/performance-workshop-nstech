using PerformanceApi.Models.Entity;
namespace PerformanceApi.Repositories;

public interface IStatementRepository
{
    Task<IEnumerable<Transaction>> GetTransactionsByAccountAsync(int accountId, DateTime startDate, DateTime endDate);
}