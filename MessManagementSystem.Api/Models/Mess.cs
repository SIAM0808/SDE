namespace MessManagementSystem.Api.Models;

public class Mess
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string MessCode { get; set; } = string.Empty;

    public int AdminMemberId { get; set; }

    // The member who administers this mess
    public Member? AdminMember { get; set; }

    // Members who belong to this mess
    public ICollection<Member> Members { get; set; }
        = new List<Member>();

    public ICollection<MessJoinRequest> JoinRequests { get; set; }
        = new List<MessJoinRequest>();
    public ICollection<Expense> Expenses { get; set; }
        = new List<Expense>();
    public ICollection<MemberPayment> MemberPayments { get; set; }
        = new List<MemberPayment>();

    public ICollection<MemberCashTransfer> MemberCashTransfers { get; set; }
    = new List<MemberCashTransfer>();
}