
using MessManagementSystem.Api.DTOs.MemberPayment;
using MessManagementSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MessManagementSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MemberPaymentController : ControllerBase
{
    private readonly MemberPaymentService _paymentService;

    public MemberPaymentController(
        MemberPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    // Admin records a member payment
    [Authorize]
    [HttpPost("{messId}")]
    public async Task<IActionResult> CreatePayment(
        int messId,
        CreateMemberPaymentRequest request)
    {
        var memberIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (memberIdClaim == null)
        {
            return Unauthorized();
        }

        var adminMemberId = int.Parse(memberIdClaim);

        try
        {
            var payment =
                await _paymentService.CreatePaymentAsync(
                    messId,
                    adminMemberId,
                    request);

            return Ok(new
            {
                message = "Member payment recorded successfully.",
                payment = new
                {
                    id = payment.Id,
                    messId = payment.MessId,
                    memberId = payment.MemberId,
                    amount = payment.Amount,
                    paymentDate = payment.PaymentDate,
                    recordedBy = payment.RecordedBy
                }
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new
            {
                message = ex.Message
            });
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
