using PerformanceApi.Models.Dto;
namespace PerformanceApi.Services;

public interface IStatementService
{
    Task<StatementResponseDto> GetStatementAsync(int accountId, DateTime startDate, DateTime endDate);
}