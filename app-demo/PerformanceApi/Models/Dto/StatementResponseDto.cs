namespace PerformanceApi.Models.Dto;

public class StatementResponseDto
{
    public decimal Balance { get; set; }
    public TransactionDto[] Transactions { get; set; }
}