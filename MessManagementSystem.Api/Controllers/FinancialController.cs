
using MessManagementSystem.Api.DTOs.Financial;
using MessManagementSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MessManagementSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FinancialController : ControllerBase
{
    private readonly FinancialService _financialService;

    public FinancialController(FinancialService financialService)
    {
        _financialService = financialService;
    }

    [HttpGet("member-summary")]
    public async Task<ActionResult<MemberFinancialSummaryResponse>>
        GetMemberFinancialSummary(
            [FromQuery] int messId,
            [FromQuery] int memberId,
            [FromQuery] int year,
            [FromQuery] int month)
    {
        try
        {
            var summary = await _financialService
                .GetMemberFinancialSummaryAsync(
                    messId,
                    memberId,
                    year,
                    month);

            return Ok(summary);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
}
