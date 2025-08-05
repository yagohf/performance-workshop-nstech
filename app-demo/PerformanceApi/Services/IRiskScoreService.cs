using PerformanceApi.Models.Entity;

namespace PerformanceApi.Services;

public interface IRiskScoreService
{
    decimal ComputeRiskScore(Transaction currentTransaction, IEnumerable<Transaction> allTransactions);
}