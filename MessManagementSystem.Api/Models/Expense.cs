
namespace MessManagementSystem.Api.Models;

public class Expense
{
    public int Id { get; set; }

    // The mess this expense belongs to
    public int MessId { get; set; }

    // Optional member associated with this expense
    // Used for member-specific expenses such as approved cash transfers
    public int? MemberId { get; set; }

    public Member? Member { get; set; }



    public Mess? Mess { get; set; }

    // What the expense was for
    public string Description { get; set; } = string.Empty;

    // Food, HouseRent, Chief, Others
    public string Category { get; set; } = string.Empty;

    // Expense amount
    public decimal Amount { get; set; }

    // Date of the expense
    public DateTime ExpenseDate { get; set; }

    // Member/admin who recorded the expense
    public int RecordedBy { get; set; }

    public Member? RecordedByMember { get; set; }
}

