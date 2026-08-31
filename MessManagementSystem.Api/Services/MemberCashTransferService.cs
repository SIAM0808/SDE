
using MessManagementSystem.Api.Data;
using MessManagementSystem.Api.DTOs.MemberCashTransfer;
using MessManagementSystem.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MessManagementSystem.Api.Services;

public class MemberCashTransferService
{
    private readonly MessDbContext _context;

    public MemberCashTransferService(MessDbContext context)
    {
        _context = context;
    }

    // Admin creates a pending cash transfer
    public async Task<MemberCashTransfer> CreateTransferAsync(
        int messId,
        int adminMemberId,
        CreateMemberCashTransferRequest request)
    {
        // 1. Check whether the mess exists
        var mess = await _context.Messes
            .FirstOrDefaultAsync(m => m.Id == messId);

        if (mess == null)
        {
            throw new InvalidOperationException(
                "Mess not found.");
        }

        // 2. Only the mess admin can create a transfer
        if (mess.AdminMemberId != adminMemberId)
        {
            throw new UnauthorizedAccessException(
                "Only the mess admin can give money to a member.");
        }

        // 3. Validate amount
        if (request.Amount <= 0)
        {
            throw new InvalidOperationException(
                "Amount must be greater than zero.");
        }

        // 4. Check whether the member exists
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == request.MemberId);

        if (member == null)
        {
            throw new InvalidOperationException(
                "Member not found.");
        }

        // 5. Member must belong to this mess
        if (member.MessId != messId)
        {
            throw new InvalidOperationException(
                "This member does not belong to this mess.");
        }

        // 6. Create pending transfer
        var transfer = new MemberCashTransfer
        {
            MessId = messId,
            MemberId = request.MemberId,
            Amount = request.Amount,
            TransferDate = request.TransferDate.Date,
            RecordedBy = adminMemberId,
            Status = "Pending",
            ApprovedAt = null
        };

        _context.MemberCashTransfers.Add(transfer);

        // 7. Save
        await _context.SaveChangesAsync();

        return transfer;
    }


    // Get pending cash transfers for a member
    public async Task<List<MemberCashTransfer>> GetMyPendingTransfersAsync(
        int memberId)
    {
        // 1. Check whether the member exists
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == memberId);

        if (member == null)
        {
            throw new InvalidOperationException(
                "Member not found.");
        }

        // 2. Member must belong to a mess
        if (member.MessId == null)
        {
            throw new InvalidOperationException(
                "You must belong to a mess to view cash transfers.");
        }

        // 3. Get pending transfers for this member
        var transfers = await _context.MemberCashTransfers
            .Where(t =>
                t.MemberId == memberId &&
                t.Status == "Pending")
            .OrderByDescending(t => t.TransferDate)
            .ToListAsync();

        return transfers;
    }

    // Member approves a cash transfer
    public async Task<MemberCashTransfer> ApproveTransferAsync(
        int transferId,
        int memberId)
    {
        // 1. Find the transfer
        var transfer = await _context.MemberCashTransfers
            .FirstOrDefaultAsync(t => t.Id == transferId);

        if (transfer == null)
        {
            throw new InvalidOperationException(
                "Cash transfer not found.");
        }

        // 2. Only the receiving member can approve
        if (transfer.MemberId != memberId)
        {
            throw new UnauthorizedAccessException(
                "You are not authorized to approve this cash transfer.");
        }

        // 3. Transfer must still be pending
        if (transfer.Status != "Pending")
        {
            throw new InvalidOperationException(
                "This cash transfer has already been processed.");
        }


        // 4. Approve the transfer
        transfer.Status = "Approved";
        transfer.ApprovedAt = DateTime.UtcNow;

        // 5. Create an expense record
        // because the mess has actually given this money
        var expense = new Expense
        {
            MessId = transfer.MessId,
            MemberId = transfer.MemberId,
            Description = "Cash given to member",
            Category = "MemberCashTransfer",
            Amount = transfer.Amount,
            ExpenseDate = transfer.TransferDate.Date,
            RecordedBy = transfer.RecordedBy
        };

        _context.Expenses.Add(expense);

        // 6. Save everything together
        await _context.SaveChangesAsync();

        return transfer;


    }
    // Member rejects a cash transfer
    public async Task<MemberCashTransfer> RejectTransferAsync(
        int transferId,
        int memberId)
    {
        // 1. Find the transfer
        var transfer = await _context.MemberCashTransfers
            .FirstOrDefaultAsync(t => t.Id == transferId);

        if (transfer == null)
        {
            throw new InvalidOperationException(
                "Cash transfer not found.");
        }

        // 2. Only the receiving member can reject
        if (transfer.MemberId != memberId)
        {
            throw new UnauthorizedAccessException(
                "You are not authorized to reject this cash transfer.");
        }

        // 3. Transfer must still be pending
        if (transfer.Status != "Pending")
        {
            throw new InvalidOperationException(
                "This cash transfer has already been processed.");
        }

        // 4. Reject the transfer
        transfer.Status = "Rejected";
        transfer.ApprovedAt = null;

        // 5. Save
        await _context.SaveChangesAsync();

        return transfer;
    }



}
