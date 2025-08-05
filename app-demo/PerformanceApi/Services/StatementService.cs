using System.Collections;
using PerformanceApi.Models.Dto;
using PerformanceApi.Models.Entity;
using PerformanceApi.Repositories;

namespace PerformanceApi.Services;

public class StatementService : IStatementService
{
    private readonly IStatementRepository _repository;
    private readonly IRiskScoreService _riskScoreService;

    public StatementService(IStatementRepository repository, IRiskScoreService riskScoreService)
    {
        _repository = repository;
        _riskScoreService = riskScoreService;
    }

    public async Task<StatementResponseDto> GetStatementAsync(int accountId, DateTime startDate, DateTime endDate)
    {
        // Buscar no BD
        var transactions = await _repository.GetTransactionsByAccountAsync(accountId, startDate, endDate);
        
        // Mapear a resposta
        var response = new StatementResponseDto();
        response.Transactions = transactions.Select(t => TransactionToDto(t, transactions));
        
        // Devolver o resultado
        return response;
    }

    private TransactionDto TransactionToDto(Transaction transaction, IEnumerable<Transaction> transactions)
    {
        var transactionDto = new TransactionDto
        {
            Amount = transaction.Amount,
            Date = transaction.TransactionDate,
            Description = transaction.Description,
            CategoryName = transaction.Category?.Name,
            RiskScore = _riskScoreService.ComputeRiskScore(transaction, transactions)
        };
        
        return transactionDto;
    }
}