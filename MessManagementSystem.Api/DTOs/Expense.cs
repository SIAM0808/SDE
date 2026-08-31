
namespace MessManagementSystem.Api.DTOs.Expense;

public class CreateExpenseRequest
{
    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime ExpenseDate { get; set; }
}
