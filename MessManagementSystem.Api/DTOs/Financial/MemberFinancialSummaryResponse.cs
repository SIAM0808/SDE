namespace MessManagementSystem.Api.DTOs.Financial;

public class MemberFinancialSummaryResponse
{
    public int MemberId { get; set; }

    public string MemberName { get; set; } = string.Empty;

    // Total money paid by the member
    public decimal GivenMoney { get; set; }

    // Member's share of house rent
    public decimal HouseRent { get; set; }

    // Member's share of chief bill
    public decimal ChiefBill { get; set; }

    // Member's share of other expenses
    public decimal OthersBill { get; set; }

    // Total meals consumed by the member
    public int TotalMeals { get; set; }

    // Current meal rate
    public decimal MealRate { get; set; }

    // Total cost of this member's meals
    public decimal MealCost { get; set; }

    // Cash transfers made to the member
    public decimal CashTransfers { get; set; }

    // Total expense charged to the member
    public decimal TotalExpense { get; set; }

    // Money remaining (+) or due (-)
    public decimal Due { get; set; }

    // Whether the member is allowed to order meals
    public bool CanOrderMeal { get; set; }

    // Whether the member can leave / be removed
    public bool CanLeaveOrBeRemoved { get; set; }
}