namespace MessManagementSystem.Api.DTOs.Expense;

public class ExpenseResponse
{
    public int Id { get; set; }

    public int MessId { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime ExpenseDate { get; set; }

    public int RecordedBy { get; set; }
}
