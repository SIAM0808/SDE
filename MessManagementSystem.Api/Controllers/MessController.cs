using MessManagementSystem.Api.Data;
using MessManagementSystem.Api.DTOs.Mess;
using MessManagementSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MessManagementSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessController : ControllerBase
{
    private readonly MessService _messService;
    private readonly MessDbContext _context;

    public MessController(MessService messService, MessDbContext context)
    {
        _messService = messService;
        _context = context;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateMess(
        CreateMessRequest request)
    {
        var memberIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (memberIdClaim == null)
        {
            return Unauthorized();
        }

        var memberId = int.Parse(memberIdClaim);

        try
        {
            var mess = await _messService.CreateMessAsync(
                request,
                memberId);

            return Ok(new
            {
                message = "Mess created successfully.",
                messId = mess.Id,
                name = mess.Name,
                messCode = mess.MessCode,
                adminMemberId = mess.AdminMemberId
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

    // Mess details want to know
    // [Authorize]
    [HttpGet("search")]
    public async Task<IActionResult> SearchMess(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(new
            {
                message = "Search query is required."
            });
        }

        var messes = await _context.Messes
            .Where(m =>
                m.Name.Contains(query) ||
                m.MessCode.Contains(query))
            .Select(m => new
            {
                id = m.Id,
                name = m.Name,
                messCode = m.MessCode,
                adminMemberId = m.AdminMemberId,
                memberCount = m.Members.Count
            })
            .ToListAsync();

        return Ok(messes);
    }



    // Update mess details
    [Authorize]
    [HttpPut("{messId}")]
    public async Task<IActionResult> UpdateMess(
    int messId,
    UpdateMessRequest request)
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
            var mess = await _messService.UpdateMessAsync(
                messId,
                memberId,
                request);

            if (mess == null)
            {
                return NotFound(new
                {
                    message = "Mess not found."
                });
            }

            return Ok(new
            {
                message = "Mess updated successfully.",
                messId = mess.Id,
                name = mess.Name,
                messCode = mess.MessCode
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


    // Join endpoint

    [Authorize]
    [HttpPost("join")]
    public async Task<IActionResult> SendJoinRequest(
    JoinMessRequest request)
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
            var joinRequest =
                await _messService.SendJoinRequestAsync(
                    memberId,
                    request.MessId);

            return Ok(new
            {
                message = "Join request sent successfully.",
                requestId = joinRequest.Id,
                messId = joinRequest.MessId,
                memberId = joinRequest.MemberId,
                status = joinRequest.Status,
                requestDate = joinRequest.RequestDate
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


    // Get join requests for a mess
    [Authorize]
    [HttpGet("{messId}/join-requests")]
    public async Task<IActionResult> GetJoinRequests(int messId)
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
            var requests = await _messService.GetJoinRequestsAsync(messId, memberId);

            return Ok(requests);
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
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }



    // Approve a join request
    [Authorize]
    [HttpPost("{messId}/join-requests/approve")]
    public async Task<IActionResult> ApproveJoinRequest(
    int messId,
    ApproveJoinRequest request)
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
            var joinRequest =
                await _messService.ApproveJoinRequestAsync(
                    messId,
                    request.RequestId,
                    adminMemberId);

            return Ok(new
            {
                message = "Join request approved successfully.",
                requestId = joinRequest.Id,
                messId = joinRequest.MessId,
                memberId = joinRequest.MemberId,
                status = joinRequest.Status
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



    // Reject a join request
    [Authorize]
    [HttpPost("{messId}/join-requests/reject")]
    public async Task<IActionResult> RejectJoinRequest(
    int messId,
    RejectJoinRequest request)
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
            var joinRequest =
                await _messService.RejectJoinRequestAsync(
                    messId,
                    request.RequestId,
                    adminMemberId);

            return Ok(new
            {
                message = "Join request rejected successfully.",
                requestId = joinRequest.Id,
                messId = joinRequest.MessId,
                memberId = joinRequest.MemberId,
                status = joinRequest.Status
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



    // Admin removes a member
    [Authorize]
    [HttpDelete("{messId}/members/{memberId}")]
    public async Task<IActionResult> RemoveMember(
        int messId,
        int memberId)
    {
        var adminMemberIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (adminMemberIdClaim == null)
        {
            return Unauthorized();
        }

        var adminMemberId = int.Parse(adminMemberIdClaim);

        try
        {
            await _messService.RemoveMemberAsync(
                messId,
                adminMemberId,
                memberId);

            return Ok(new
            {
                message = "Member removed from the mess successfully."
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

    // Member leaves mess
    [Authorize]
    [HttpPost("{messId}/leave")]
    public async Task<IActionResult> LeaveMess(int messId)
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
            await _messService.LeaveMessAsync(
                messId,
                memberId);

            return Ok(new
            {
                message = "You have successfully left the mess."
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

    // List all members of a mess
    [Authorize]
    [HttpGet("{messId}/members")]
    public async Task<IActionResult> GetMessMembers(int messId)
    {
        var members = await _context.Members
            .Where(m => m.MessId == messId)
            .Select(m => new
            {
                id = m.Id,
                name = m.Name,
                email = m.Email,
                phone = m.Phone,
                joinDate = m.JoinDate
            })
            .ToListAsync();

        return Ok(members);
    }

    // Admin deletes a mess
    [Authorize]
    [HttpDelete("{messId}")]
    public async Task<IActionResult> DeleteMess(int messId)
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
            await _messService.DeleteMessAsync(
                messId,
                adminMemberId);

            return Ok(new
            {
                message = "Mess deleted successfully."
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