
namespace MessManagementSystem.Api.Models;

public class MemberCashTransfer
{
    public int Id { get; set; }

    // The mess giving the money
    public int MessId { get; set; }

    public Mess? Mess { get; set; }

    // The member receiving the money
    public int MemberId { get; set; }

    public Member? Member { get; set; }

    // Amount given to the member
    public decimal Amount { get; set; }

    // Date the transfer was created
    public DateTime TransferDate { get; set; }

    // Admin who created the transfer
    public int RecordedBy { get; set; }

    public Member? RecordedByMember { get; set; }

    // Pending, Approved or Rejected
    public string Status { get; set; } = "Pending";

    // When the member approved the transfer
    public DateTime? ApprovedAt { get; set; }
}

