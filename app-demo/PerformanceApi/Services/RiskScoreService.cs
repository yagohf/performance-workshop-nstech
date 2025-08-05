using PerformanceApi.Models.Entity;

namespace PerformanceApi.Services;

public class RiskScoreService : IRiskScoreService
{
    public decimal ComputeRiskScore(Transaction currentTransaction, IEnumerable<Transaction> allTransactions)
    {
        return 0;
        
        // var riskScore = allTransactions.Count(other =>
        //     other.Id != currentTransaction.Id && // Não comparar a transação com ela mesma
        //     other.TransactionDate.Date == currentTransaction.TransactionDate.Date &&
        //     Math.Abs(other.Amount - currentTransaction.Amount) <= 1.0m);
        //
        // return riskScore;
    }
}