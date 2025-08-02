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
        // #################################################
        // ### PROBLEMA N+1 - PONTO DE ORIGEM            ###
        // #################################################
        // A consulta busca as transações, mas NÃO INCLUI os dados da Categoria.
        // O Entity Framework não fará o JOIN. O problema será disparado na camada de serviço
        // quando o código tentar acessar a propriedade "transaction.Category.Name".
        var transactions = await _context.Transactions
            .AsNoTracking()
            .Include(t=> t.Category)
            .Where(t => t.AccountId == accountId && t.TransactionDate >= startDate && t.TransactionDate <= endDate)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();

        return transactions;
    }
}