using Microsoft.AspNetCore.Mvc;
using PerformanceApi.Services;

namespace PerformanceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatementController : ControllerBase
{
    private readonly IStatementService _statementService;

    public StatementController(IStatementService statementService)
    {
        _statementService = statementService;
    }

    [HttpGet("{accountId}")]
    public async Task<IActionResult> GetStatement(int accountId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        if (endDate == DateTime.MinValue) endDate = DateTime.Now;
        if (startDate == DateTime.MinValue) startDate = endDate.AddMonths(-1);

        var statement = await _statementService.GetStatementAsync(accountId, startDate, endDate);
        return Ok(statement);
    }
}