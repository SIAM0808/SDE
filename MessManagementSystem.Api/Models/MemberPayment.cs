
namespace MessManagementSystem.Api.Models;

public class MemberPayment
{
    public int Id { get; set; }

    // The mess receiving the payment
    public int MessId { get; set; }

    public Mess? Mess { get; set; }

    // The member who paid the money
    public int MemberId { get; set; }

    public Member? Member { get; set; }

    // Amount paid
    public decimal Amount { get; set; }

    // Date the payment was made
    public DateTime PaymentDate { get; set; }

    // Admin who recorded the payment
    public int RecordedBy { get; set; }

    public Member? RecordedByMember { get; set; }
}

