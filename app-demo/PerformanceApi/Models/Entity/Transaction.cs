namespace PerformanceApi.Models.Entity;

public class Transaction
{
    public long Id { get; set; }
    public int AccountId { get; set; }
    public int CategoryId { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public byte[] Protocol { get; set; } = [];

    // Propriedade de navegação para o EF Core
    public virtual Category Category { get; set; } = null!;
}