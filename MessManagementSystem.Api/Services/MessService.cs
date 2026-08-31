using MessManagementSystem.Api.Data;
using MessManagementSystem.Api.DTOs.Mess;
using MessManagementSystem.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MessManagementSystem.Api.Services;

public class MessService
{
    private readonly MessDbContext _context;
    private readonly FinancialService _financialService;

    public MessService(MessDbContext context, FinancialService financialService)
    {
        _context = context;
        _financialService = financialService;
    }

    public async Task<Mess> CreateMessAsync(
        CreateMessRequest request,
        int memberId)
    {
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == memberId);

        if (member == null)
        {
            throw new InvalidOperationException("Member not found.");
        }

        if (member.MessId != null)
        {
            throw new InvalidOperationException(
                "You are already a member of a mess.");
        }

        var messCode = await GenerateUniqueMessCodeAsync();

        var mess = new Mess
        {
            Name = request.Name,
            MessCode = messCode,
            AdminMemberId = member.Id
        };

        _context.Messes.Add(mess);

        await _context.SaveChangesAsync();

        member.MessId = mess.Id;

        await _context.SaveChangesAsync();

        return mess;
    }

    private async Task<string> GenerateUniqueMessCodeAsync()
    {
        string messCode;

        do
        {
            messCode = Random.Shared
                .Next(100000, 1000000)
                .ToString();

        } while (await _context.Messes
            .AnyAsync(m => m.MessCode == messCode));

        return messCode;
    }



    // Update mess details
    public async Task<Mess?> UpdateMessAsync(
    int messId,
    int memberId,
    UpdateMessRequest request)
    {
        var mess = await _context.Messes
            .FirstOrDefaultAsync(m => m.Id == messId);

        if (mess == null)
        {
            return null;
        }

        if (mess.AdminMemberId != memberId)
        {
            throw new UnauthorizedAccessException(
                "Only the mess admin can update the mess.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new InvalidOperationException(
                "Mess name is required.");
        }

        mess.Name = request.Name.Trim();

        await _context.SaveChangesAsync();

        return mess;
    }


    // Join request hanlder
    public async Task<MessJoinRequest> SendJoinRequestAsync(
    int memberId,
    int messId)
    {
        // 1. Check whether the member exists
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == memberId);

        if (member == null)
        {
            throw new InvalidOperationException("Member not found.");
        }

        // 2. Member cannot join another mess if already in a mess
        if (member.MessId != null)
        {
            throw new InvalidOperationException(
                "You are already a member of a mess.");
        }

        // 3. Check whether the mess exists
        var mess = await _context.Messes
            .FirstOrDefaultAsync(m => m.Id == messId);

        if (mess == null)
        {
            throw new InvalidOperationException("Mess not found.");
        }

        // 4. Check whether the member already has a pending request
        var existingRequest = await _context.MessJoinRequests
            .FirstOrDefaultAsync(r =>
                r.MemberId == memberId &&
                r.MessId == messId &&
                r.Status == "Pending");

        if (existingRequest != null)
        {
            throw new InvalidOperationException(
                "You have already sent a join request to this mess.");
        }

        // 5. Create the join request
        var joinRequest = new MessJoinRequest
        {
            MemberId = memberId,
            MessId = messId,
            Status = "Pending",
            RequestDate = DateTime.UtcNow
        };

        _context.MessJoinRequests.Add(joinRequest);

        await _context.SaveChangesAsync();

        return joinRequest;
    }


    public async Task<List<JoinRequestResponse>> GetJoinRequestsAsync(int messId, int memberId)
    {
        // Check whether the mess exists
        var mess = await _context.Messes
            .FirstOrDefaultAsync(m => m.Id == messId);

        if (mess == null)
        {
            throw new InvalidOperationException("Mess not found.");
        }

        // Only the mess admin can see join requests
        if (mess.AdminMemberId != memberId)
        {
            throw new UnauthorizedAccessException(
                "Only the mess admin can view join requests.");
        }

        // Get all join requests with member information
        var requests = await _context.MessJoinRequests
            .Where(r => r.MessId == messId)
            .Include(r => r.Member)
            .Select(r => new JoinRequestResponse
            {
                Id = r.Id,
                MessId = r.MessId,
                MemberId = r.MemberId,
                MemberName = r.Member!.Name,
                MemberEmail = r.Member!.Email,
                Status = r.Status,
                RequestDate = r.RequestDate
            })
            .OrderByDescending(r => r.RequestDate)
            .ToListAsync();

        return requests;
    }


    // Approve a join request
    public async Task<MessJoinRequest> ApproveJoinRequestAsync(
    int messId,
    int requestId,
    int adminMemberId)
    {
        // 1. Check whether the mess exists
        var mess = await _context.Messes
            .FirstOrDefaultAsync(m => m.Id == messId);

        if (mess == null)
        {
            throw new InvalidOperationException("Mess not found.");
        }

        // 2. Check whether the logged-in member is the mess admin
        if (mess.AdminMemberId != adminMemberId)
        {
            throw new UnauthorizedAccessException(
                "Only the mess admin can approve join requests.");
        }

        // 3. Find the join request
        var joinRequest = await _context.MessJoinRequests
            .FirstOrDefaultAsync(r =>
                r.Id == requestId &&
                r.MessId == messId);

        if (joinRequest == null)
        {
            throw new InvalidOperationException(
                "Join request not found.");
        }

        // 4. Request must still be pending
        if (joinRequest.Status != "Pending")
        {
            throw new InvalidOperationException(
                "This join request has already been processed.");
        }

        // 5. Find the requesting member
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == joinRequest.MemberId);

        if (member == null)
        {
            throw new InvalidOperationException(
                "Member not found.");
        }

        // 6. Make sure the member is not already in another mess
        if (member.MessId != null)
        {
            throw new InvalidOperationException(
                "This member is already a member of a mess.");
        }

        // 7. Add member to this mess
        member.MessId = messId;

        // 8. Approve this request
        joinRequest.Status = "Approved";

        // 9. Delete all other join requests of this member
        var otherRequests = await _context.MessJoinRequests
            .Where(r =>
                r.MemberId == member.Id &&
                r.Id != joinRequest.Id)
            .ToListAsync();

        _context.MessJoinRequests.RemoveRange(otherRequests);

        // 10. Save everything
        await _context.SaveChangesAsync();

        return joinRequest;
    }


    // Reject a join request
    public async Task<MessJoinRequest> RejectJoinRequestAsync(
    int messId,
    int requestId,
    int adminMemberId)
    {
        // 1. Check whether the mess exists
        var mess = await _context.Messes
            .FirstOrDefaultAsync(m => m.Id == messId);

        if (mess == null)
        {
            throw new InvalidOperationException("Mess not found.");
        }

        // 2. Check whether the current member is the mess admin
        if (mess.AdminMemberId != adminMemberId)
        {
            throw new UnauthorizedAccessException(
                "Only the mess admin can reject join requests.");
        }

        // 3. Find the join request
        var joinRequest = await _context.MessJoinRequests
            .FirstOrDefaultAsync(r =>
                r.Id == requestId &&
                r.MessId == messId);

        if (joinRequest == null)
        {
            throw new InvalidOperationException(
                "Join request not found.");
        }

        // 4. Check whether the request is still pending
        if (joinRequest.Status != "Pending")
        {
            throw new InvalidOperationException(
                "This join request has already been processed.");
        }

        // 5. Reject the request
        joinRequest.Status = "Rejected";

        await _context.SaveChangesAsync();

        return joinRequest;
    }



    // Admin removes a member from the mess
    public async Task RemoveMemberAsync(
        int messId,
        int adminMemberId,
        int memberId)
    {
        // 1. Check whether the mess exists
        var mess = await _context.Messes
            .FirstOrDefaultAsync(m => m.Id == messId);

        if (mess == null)
        {
            throw new InvalidOperationException("Mess not found.");
        }

        // 2. Check whether the logged-in member is the mess admin
        if (mess.AdminMemberId != adminMemberId)
        {
            throw new UnauthorizedAccessException(
                "Only the mess admin can remove members.");
        }

        // 3. Find the member to be removed
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == memberId);

        if (member == null)
        {
            throw new InvalidOperationException("Member not found.");
        }

        // 4. Make sure the member belongs to this mess
        if (member.MessId != messId)
        {
            throw new InvalidOperationException(
                "This member does not belong to this mess.");
        }

        // 5. Admin cannot remove themselves
        if (member.Id == adminMemberId)
        {
            throw new InvalidOperationException(
                "The mess admin cannot remove themselves.");
        }

        // 6. Check financial balance (using current month, same as financial summary UI)
        var now = DateTime.Now;
        var summary = await _financialService.GetMemberFinancialSummaryAsync(
            messId, memberId, now.Year, now.Month);
        if (summary.Due != 0)
        {
            throw new InvalidOperationException(
                "Cannot remove member because their financial balance is not settled. Outstanding due/receivable: " + summary.Due);
        }

        // 7. Remove the member from the mess
        member.MessId = null;

        // 8. Save changes
        await _context.SaveChangesAsync();
    }




    // Member leaves mess
    public async Task LeaveMessAsync(
        int messId,
        int memberId)
    {
        // 1. Check whether the mess exists
        var mess = await _context.Messes
            .FirstOrDefaultAsync(m => m.Id == messId);

        if (mess == null)
        {
            throw new InvalidOperationException("Mess not found.");
        }

        // 2. Find the member
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == memberId);

        if (member == null)
        {
            throw new InvalidOperationException("Member not found.");
        }

        // 3. Check whether the member belongs to this mess
        if (member.MessId != messId)
        {
            throw new InvalidOperationException(
                "You are not a member of this mess.");
        }

        // 4. Admin cannot leave directly
        if (mess.AdminMemberId == memberId)
        {
            throw new UnauthorizedAccessException(
                "The mess admin cannot leave the mess directly.");
        }

        // 5. Check financial balance (using current month, same as financial summary UI)
        var now = DateTime.Now;
        var summary = await _financialService.GetMemberFinancialSummaryAsync(
            messId, memberId, now.Year, now.Month);
        if (summary.Due != 0)
        {
            throw new InvalidOperationException(
                "Cannot leave mess because your financial balance is not settled. Outstanding due/receivable: " + summary.Due);
        }

        // 6. Remove the member from the mess
        member.MessId = null;

        // 7. Save changes
        await _context.SaveChangesAsync();
    }

    // Admin deletes the mess
    public async Task DeleteMessAsync(
        int messId,
        int adminMemberId)
    {
        // 1. Check whether the mess exists
        var mess = await _context.Messes
            .Include(m => m.Members)
            .FirstOrDefaultAsync(m => m.Id == messId);

        if (mess == null)
        {
            throw new InvalidOperationException("Mess not found.");
        }

        // 2. Check whether the logged-in member is the mess admin
        if (mess.AdminMemberId != adminMemberId)
        {
            throw new UnauthorizedAccessException(
                "Only the mess admin can delete the mess.");
        }

        // 3. Ensure all members have settled dues (using current month, same as financial summary UI)
        var now = DateTime.Now;
        foreach (var member in mess.Members)
        {
            var memberSummary = await _financialService.GetMemberFinancialSummaryAsync(
                messId, member.Id, now.Year, now.Month);
            if (memberSummary.Due != 0)
            {
                throw new InvalidOperationException(
                    $"Cannot delete mess. Member {member.Name} has an unsettled balance of {memberSummary.Due}.");
            }
        }

        // 4. Detach all members from the mess
        foreach (var member in mess.Members)
        {
            member.MessId = null;
        }

        // 5. Delete the mess
        _context.Messes.Remove(mess);

        // 6. Save changes
        await _context.SaveChangesAsync();
    }
}