
using MessManagementSystem.Api.DTOs.MemberCashTransfer;
using MessManagementSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MessManagementSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MemberCashTransferController : ControllerBase
{
    private readonly MemberCashTransferService _transferService;

    public MemberCashTransferController(
        MemberCashTransferService transferService)
    {
        _transferService = transferService;
    }

    // Admin gives money to a member
    [Authorize]
    [HttpPost("{messId}")]
    public async Task<IActionResult> CreateTransfer(
        int messId,
        CreateMemberCashTransferRequest request)
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
            var transfer =
                await _transferService.CreateTransferAsync(
                    messId,
                    adminMemberId,
                    request);

            return Ok(new
            {
                message = "Cash transfer created successfully. Waiting for member approval.",
                transfer = new
                {
                    id = transfer.Id,
                    messId = transfer.MessId,
                    memberId = transfer.MemberId,
                    amount = transfer.Amount,
                    transferDate = transfer.TransferDate,
                    recordedBy = transfer.RecordedBy,
                    status = transfer.Status,
                    approvedAt = transfer.ApprovedAt
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



    // Member views pending cash transfers
    [Authorize]
    [HttpGet("my-pending")]
    public async Task<IActionResult> GetMyPendingTransfers()
    {
        var memberIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (memberIdClaim == null)
        {
            return Unauthorized();
        }

        var memberId = int.Parse(memberIdClaim);

        try
        {
            var transfers =
                await _transferService.GetMyPendingTransfersAsync(
                    memberId);

            return Ok(transfers.Select(t => new
            {
                id = t.Id,
                messId = t.MessId,
                memberId = t.MemberId,
                amount = t.Amount,
                transferDate = t.TransferDate,
                status = t.Status
            }));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }


    // Member approves a cash transfer
    [Authorize]
    [HttpPost("{transferId}/approve")]
    public async Task<IActionResult> ApproveTransfer(
        int transferId)
    {
        var memberIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (memberIdClaim == null)
        {
            return Unauthorized();
        }

        var memberId = int.Parse(memberIdClaim);

        try
        {
            var transfer =
                await _transferService.ApproveTransferAsync(
                    transferId,
                    memberId);

            return Ok(new
            {
                message = "Cash transfer approved successfully.",
                transfer = new
                {
                    id = transfer.Id,
                    messId = transfer.MessId,
                    memberId = transfer.MemberId,
                    amount = transfer.Amount,
                    transferDate = transfer.TransferDate,
                    status = transfer.Status,
                    approvedAt = transfer.ApprovedAt
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


    // Member rejects a cash transfer
    [Authorize]
    [HttpPost("{transferId}/reject")]
    public async Task<IActionResult> RejectTransfer(
        int transferId)
    {
        var memberIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (memberIdClaim == null)
        {
            return Unauthorized();
        }

        var memberId = int.Parse(memberIdClaim);

        try
        {
            var transfer =
                await _transferService.RejectTransferAsync(
                    transferId,
                    memberId);

            return Ok(new
            {
                message = "Cash transfer rejected successfully.",
                transfer = new
                {
                    id = transfer.Id,
                    messId = transfer.MessId,
                    memberId = transfer.MemberId,
                    amount = transfer.Amount,
                    transferDate = transfer.TransferDate,
                    status = transfer.Status,
                    approvedAt = transfer.ApprovedAt
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
