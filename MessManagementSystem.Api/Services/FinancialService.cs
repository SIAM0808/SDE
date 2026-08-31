using MessManagementSystem.Api.Data;
using MessManagementSystem.Api.DTOs.Financial;
using Microsoft.EntityFrameworkCore;

namespace MessManagementSystem.Api.Services;

public class FinancialService
{
    private readonly MessDbContext _context;

    public FinancialService(MessDbContext context)
    {
        _context = context;
    }

    public async Task<MemberFinancialSummaryResponse>
        GetMemberFinancialSummaryAsync(
            int messId,
            int memberId,
            int year,
            int month)
    {
        // 1. Validate month
        if (month < 1 || month > 12)
        {
            throw new InvalidOperationException(
                "Invalid month.");
        }

        // 2. Check member
        var member = await _context.Members
            .FirstOrDefaultAsync(m =>
                m.Id == memberId &&
                m.MessId == messId);

        if (member == null)
        {
            throw new InvalidOperationException(
                "Member not found in this mess.");
        }

        // 3. Determine month range
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        // 4. Count members currently in the mess
        var memberCount = await _context.Members
            .CountAsync(m => m.MessId == messId);

        if (memberCount == 0)
        {
            throw new InvalidOperationException(
                "No members found in this mess.");
        }

        // 5. Get total payments made by this member
        var givenMoney = await _context.MemberPayments
            .Where(p =>
                p.MessId == messId &&
                p.MemberId == memberId &&
                p.PaymentDate >= startDate &&
                p.PaymentDate < endDate)
            .SumAsync(p => (decimal?)p.Amount) ?? 0;

        // 6. Get shared House Rent
        var totalHouseRent = await _context.Expenses
            .Where(e =>
                e.MessId == messId &&
                e.Category == "HouseRent" &&
                e.ExpenseDate >= startDate &&
                e.ExpenseDate < endDate)
            .SumAsync(e => (decimal?)e.Amount) ?? 0;

        // 7. Get shared Chief expense
        var totalChief = await _context.Expenses
            .Where(e =>
                e.MessId == messId &&
                e.Category == "Chief" &&
                e.ExpenseDate >= startDate &&
                e.ExpenseDate < endDate)
            .SumAsync(e => (decimal?)e.Amount) ?? 0;

        // 8. Get shared Others expense
        var totalOthers = await _context.Expenses
            .Where(e =>
                e.MessId == messId &&
                e.Category == "Others" &&
                e.ExpenseDate >= startDate &&
                e.ExpenseDate < endDate)
            .SumAsync(e => (decimal?)e.Amount) ?? 0;

        // 9. Each member's share of shared expenses
        var houseRent = totalHouseRent / memberCount;
        var chiefBill = totalChief / memberCount;
        var othersBill = totalOthers / memberCount;

        // 10. Get this member's meals
        var totalMeals = await _context.Meals
            .Where(m =>
                m.MemberId == memberId &&
                m.MealDate >= startDate &&
                m.MealDate < endDate)
            .SumAsync(m =>
                m.Breakfast +
                m.Lunch +
                m.Dinner);

        // 11. Get total meals of the entire mess
        var totalMessMeals = await _context.Meals
            .Where(m =>
                m.Member.MessId == messId &&
                m.MealDate >= startDate &&
                m.MealDate < endDate)
            .SumAsync(m =>
                m.Breakfast +
                m.Lunch +
                m.Dinner);

        // 12. Get total Food expense
        var foodExpense = await _context.Expenses
            .Where(e =>
                e.MessId == messId &&
                e.Category == "Food" &&
                e.ExpenseDate >= startDate &&
                e.ExpenseDate < endDate)
            .SumAsync(e => (decimal?)e.Amount) ?? 0;

        // 13. Calculate meal rate
        decimal mealRate = 0;

        if (totalMessMeals > 0)
        {
            mealRate = foodExpense / totalMessMeals;
        }

        // 14. Calculate this member's meal cost
        var mealCost = totalMeals * mealRate;

        // 14b. Get cash transfers for this member
        var cashTransfers = await _context.Expenses
            .Where(e =>
                e.MessId == messId &&
                e.MemberId == memberId &&
                e.Category == "MemberCashTransfer" &&
                e.ExpenseDate >= startDate &&
                e.ExpenseDate < endDate)
            .SumAsync(e => (decimal?)e.Amount) ?? 0;

        // 15. Calculate total expense
        var totalExpense =
            houseRent +
            chiefBill +
            othersBill +
            mealCost +
            cashTransfers;

        // 16. Calculate due (rounded to 2 decimal places)
        var due = Math.Round(givenMoney - totalExpense, 2);

        // 17. Meal ordering rule
        var canOrderMeal = due >= 0;

        // 18. Leaving/removal rule
        var canLeaveOrBeRemoved = due == 0;

        return new MemberFinancialSummaryResponse
        {
            MemberId = memberId,
            MemberName = member.Name,
            GivenMoney = givenMoney,

            HouseRent = houseRent,
            ChiefBill = chiefBill,
            OthersBill = othersBill,

            TotalMeals = totalMeals,
            MealRate = mealRate,
            MealCost = mealCost,
            CashTransfers = cashTransfers,

            TotalExpense = totalExpense,
            Due = due,

            CanOrderMeal = canOrderMeal,
            CanLeaveOrBeRemoved = canLeaveOrBeRemoved
        };
    }

    public async Task<decimal> GetMemberBalanceAsync(int messId, int memberId)
    {
        // 1. Check member
        var member = await _context.Members
            .FirstOrDefaultAsync(m =>
                m.Id == memberId &&
                m.MessId == messId);

        if (member == null)
        {
            throw new InvalidOperationException(
                "Member not found in this mess.");
        }

        // 2. Count members currently in the mess
        var memberCount = await _context.Members
            .CountAsync(m => m.MessId == messId);

        if (memberCount == 0)
        {
            return 0;
        }

        // 3. Get total payments made by this member (All time)
        var givenMoney = await _context.MemberPayments
            .Where(p =>
                p.MessId == messId &&
                p.MemberId == memberId)
            .SumAsync(p => (decimal?)p.Amount) ?? 0;

        // 4. Get shared expenses (All time)
        var totalHouseRent = await _context.Expenses
            .Where(e =>
                e.MessId == messId &&
                e.Category == "HouseRent")
            .SumAsync(e => (decimal?)e.Amount) ?? 0;

        var totalChief = await _context.Expenses
            .Where(e =>
                e.MessId == messId &&
                e.Category == "Chief")
            .SumAsync(e => (decimal?)e.Amount) ?? 0;

        var totalOthers = await _context.Expenses
            .Where(e =>
                e.MessId == messId &&
                e.Category == "Others")
            .SumAsync(e => (decimal?)e.Amount) ?? 0;

        var houseRent = totalHouseRent / memberCount;
        var chiefBill = totalChief / memberCount;
        var othersBill = totalOthers / memberCount;

        // 5. Get this member's meals (All time)
        var totalMeals = await _context.Meals
            .Where(m =>
                m.MemberId == memberId)
            .SumAsync(m =>
                m.Breakfast +
                m.Lunch +
                m.Dinner);

        // 6. Get total meals of the entire mess (All time)
        var totalMessMeals = await _context.Meals
            .Where(m =>
                m.Member.MessId == messId)
            .SumAsync(m =>
                m.Breakfast +
                m.Lunch +
                m.Dinner);

        // 7. Get total Food expense (All time)
        var foodExpense = await _context.Expenses
            .Where(e =>
                e.MessId == messId &&
                e.Category == "Food")
            .SumAsync(e => (decimal?)e.Amount) ?? 0;

        // 8. Calculate meal rate
        decimal mealRate = 0;

        if (totalMessMeals > 0)
        {
            mealRate = foodExpense / totalMessMeals;
        }

        var mealCost = totalMeals * mealRate;

        // 8b. Get cash transfers for this member (All time)
        var cashTransfers = await _context.Expenses
            .Where(e =>
                e.MessId == messId &&
                e.MemberId == memberId &&
                e.Category == "MemberCashTransfer")
            .SumAsync(e => (decimal?)e.Amount) ?? 0;

        // 9. Calculate total expense
        var totalExpense =
            houseRent +
            chiefBill +
            othersBill +
            mealCost +
            cashTransfers;

        // 10. Return Due (Given Money - Total Expense) rounded to 2 decimal places
        return Math.Round(givenMoney - totalExpense, 2);
    }
}