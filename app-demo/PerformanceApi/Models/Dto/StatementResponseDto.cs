namespace PerformanceApi.Models.Dto;

public class StatementResponseDto
{
    public IEnumerable<TransactionDto> Transactions { get; set; }
}