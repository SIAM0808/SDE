namespace MessManagementSystem.Api.Models;

public class Member
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public DateTime JoinDate { get; set; }

    public bool IsActive { get; set; }

    // The mess this member belongs to
    public int? MessId { get; set; }

    public Mess? Mess { get; set; }

    // The mess this member administers
    public Mess? AdminMess { get; set; }

    public ICollection<Expense> RecordedExpenses { get; set; }
    = new List<Expense>();
    public ICollection<MemberPayment> Payments { get; set; }
    = new List<MemberPayment>();

    public ICollection<MemberPayment> RecordedPayments { get; set; }
    = new List<MemberPayment>();
    public ICollection<MemberCashTransfer> CashTransfers { get; set; }
    = new List<MemberCashTransfer>();

    public ICollection<MemberCashTransfer> RecordedCashTransfers { get; set; }
    = new List<MemberCashTransfer>();
}