namespace PerformanceApi.Models.Dto;

public class TransactionDto
{
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    
    // Este campo será usado para demonstrar a alocação de memória
    public string FormattedDetails { get; set; } = string.Empty;
}