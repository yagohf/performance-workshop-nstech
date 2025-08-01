using Microsoft.EntityFrameworkCore;
using PerformanceApi.Models.Entity;

namespace PerformanceApi.Data;

public class PerformanceDbContext : DbContext
{
    public PerformanceDbContext(DbContextOptions<PerformanceDbContext> options) : base(options) { }

    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>().ToTable("Transactions");
        modelBuilder.Entity<Category>().ToTable("Categories");
        
        // Configura a relação, mas não implica Eager Loading
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId);
    }
}