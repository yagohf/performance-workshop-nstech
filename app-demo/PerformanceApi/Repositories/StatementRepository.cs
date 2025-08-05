using Microsoft.EntityFrameworkCore;
using PerformanceApi.Data;
using PerformanceApi.Models.Entity;

namespace PerformanceApi.Repositories;

public class StatementRepository : IStatementRepository
{
    private readonly PerformanceDbContext _context;

    public StatementRepository(PerformanceDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByAccountAsync(int accountId, DateTime startDate,
        DateTime endDate)
    {
        var transactions = await _context.Transactions
            .Where(t => t.AccountId == accountId && t.TransactionDate >= startDate && t.TransactionDate <= endDate)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();

        return transactions;
    }
}