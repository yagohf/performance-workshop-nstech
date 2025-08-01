namespace PerformanceApi.Models.Dto;

public class StatementResponseDto
{
    public decimal Balance { get; set; }
    public List<TransactionDto> Transactions { get; set; } = new();
}