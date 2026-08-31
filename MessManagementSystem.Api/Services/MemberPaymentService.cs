
using MessManagementSystem.Api.Data;
using MessManagementSystem.Api.DTOs.MemberPayment;
using MessManagementSystem.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MessManagementSystem.Api.Services;

public class MemberPaymentService
{
    private readonly MessDbContext _context;

    public MemberPaymentService(MessDbContext context)
    {
        _context = context;
    }

    public async Task<MemberPayment> CreatePaymentAsync(
        int messId,
        int adminMemberId,
        CreateMemberPaymentRequest request)
    {
        // 1. Check whether the mess exists
        var mess = await _context.Messes
            .FirstOrDefaultAsync(m => m.Id == messId);

        if (mess == null)
        {
            throw new InvalidOperationException(
                "Mess not found.");
        }

        // 2. Only the mess admin can record payments
        if (mess.AdminMemberId != adminMemberId)
        {
            throw new UnauthorizedAccessException(
                "Only the mess admin can record member payments.");
        }

        // 3. Validate amount
        if (request.Amount <= 0)
        {
            throw new InvalidOperationException(
                "Payment amount must be greater than zero.");
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

        // 6. Create payment
        var payment = new MemberPayment
        {
            MessId = messId,
            MemberId = request.MemberId,
            Amount = request.Amount,
            PaymentDate = request.PaymentDate.Date,
            RecordedBy = adminMemberId
        };

        _context.MemberPayments.Add(payment);

        // 7. Save
        await _context.SaveChangesAsync();

        return payment;
    }
}

