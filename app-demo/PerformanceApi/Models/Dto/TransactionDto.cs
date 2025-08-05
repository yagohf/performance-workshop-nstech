namespace PerformanceApi.Models.Dto;

public class TransactionDto
{
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal RiskScore { get; set; }
}