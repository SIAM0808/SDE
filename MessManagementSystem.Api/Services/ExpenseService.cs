
using MessManagementSystem.Api.Data;
using MessManagementSystem.Api.DTOs.Expense;
using MessManagementSystem.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MessManagementSystem.Api.Services;

public class ExpenseService
{
    private readonly MessDbContext _context;

    public ExpenseService(MessDbContext context)
    {
        _context = context;
    }

    public async Task<Expense> CreateExpenseAsync(
        int messId,
        int adminMemberId,
        CreateExpenseRequest request)
    {
        // 1. Check whether the mess exists
        var mess = await _context.Messes
            .FirstOrDefaultAsync(m => m.Id == messId);

        if (mess == null)
        {
            throw new InvalidOperationException(
                "Mess not found.");
        }

        // 2. Only the mess admin can add expenses
        if (mess.AdminMemberId != adminMemberId)
        {
            throw new UnauthorizedAccessException(
                "Only the mess admin can add expenses.");
        }

        // 3. Validate description
        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new InvalidOperationException(
                "Expense description is required.");
        }

        // 4. Validate category
        if (string.IsNullOrWhiteSpace(request.Category))
        {
            throw new InvalidOperationException(
                "Expense category is required.");
        }

        // 5. Validate amount
        if (request.Amount <= 0)
        {
            throw new InvalidOperationException(
                "Expense amount must be greater than zero.");
        }

        // 6. Validate category
        var allowedCategories = new[]
        {
            "Food",
            "HouseRent",
            "Chief",
            "Others"
        };

        if (!allowedCategories.Contains(
            request.Category.Trim(),
            StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Invalid expense category. " +
                "Allowed categories are Food, HouseRent, Chief and Others.");
        }

        // 7. Create expense
        var expense = new Expense
        {
            MessId = messId,
            Description = request.Description.Trim(),
            Category = request.Category.Trim(),
            Amount = request.Amount,
            ExpenseDate = request.ExpenseDate.Date,
            RecordedBy = adminMemberId
        };

        _context.Expenses.Add(expense);

        // 8. Save
        await _context.SaveChangesAsync();

        return expense;
    }


    // Get all expenses for a mess
    public async Task<List<ExpenseResponse>> GetExpensesAsync(
        int messId,
        int adminMemberId)
    {
        // 1. Check whether the mess exists
        var mess = await _context.Messes
            .FirstOrDefaultAsync(m => m.Id == messId);

        if (mess == null)
        {
            throw new InvalidOperationException(
                "Mess not found.");
        }

        // 2. Only the mess admin can view expenses
        if (mess.AdminMemberId != adminMemberId)
        {
            throw new UnauthorizedAccessException(
                "Only the mess admin can view expenses.");
        }

        // 3. Get all expenses
        var expenses = await _context.Expenses
            .Where(e => e.MessId == messId)
            .OrderByDescending(e => e.ExpenseDate)
            .Select(e => new ExpenseResponse
            {
                Id = e.Id,
                MessId = e.MessId,
                Description = e.Description,
                Category = e.Category,
                Amount = e.Amount,
                ExpenseDate = e.ExpenseDate,
                RecordedBy = e.RecordedBy
            })
            .ToListAsync();

        return expenses;
    }


    // Update an expense
    public async Task<Expense> UpdateExpenseAsync(
        int messId,
        int adminMemberId,
        int expenseId,
        UpdateExpenseRequest request)
    {
        // 1. Check whether the mess exists
        var mess = await _context.Messes
            .FirstOrDefaultAsync(m => m.Id == messId);

        if (mess == null)
        {
            throw new InvalidOperationException(
                "Mess not found.");
        }

        // 2. Only the mess admin can update expenses
        if (mess.AdminMemberId != adminMemberId)
        {
            throw new UnauthorizedAccessException(
                "Only the mess admin can update expenses.");
        }

        // 3. Find the expense
        var expense = await _context.Expenses
            .FirstOrDefaultAsync(e =>
                e.Id == expenseId &&
                e.MessId == messId);

        if (expense == null)
        {
            throw new InvalidOperationException(
                "Expense not found.");
        }

        // 4. Validate description
        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new InvalidOperationException(
                "Expense description is required.");
        }

        // 5. Validate category
        if (string.IsNullOrWhiteSpace(request.Category))
        {
            throw new InvalidOperationException(
                "Expense category is required.");
        }

        // 6. Validate amount
        if (request.Amount <= 0)
        {
            throw new InvalidOperationException(
                "Expense amount must be greater than zero.");
        }

        // 7. Validate category values
        var allowedCategories = new[]
        {
            "Food",
            "HouseRent",
            "Chief",
            "Others"
        };

        if (!allowedCategories.Contains(
            request.Category.Trim(),
            StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Invalid expense category. " +
                "Allowed categories are Food, HouseRent, Chief and Others.");
        }

        // 8. Update the expense
        expense.Description = request.Description.Trim();
        expense.Category = request.Category.Trim();
        expense.Amount = request.Amount;
        expense.ExpenseDate = request.ExpenseDate.Date;

        await _context.SaveChangesAsync();

        return expense;
    }


    // Delete an expense
    public async Task DeleteExpenseAsync(
        int messId,
        int adminMemberId,
        int expenseId)
    {
        // 1. Check whether the mess exists
        var mess = await _context.Messes
            .FirstOrDefaultAsync(m => m.Id == messId);

        if (mess == null)
        {
            throw new InvalidOperationException(
                "Mess not found.");
        }

        // 2. Only the mess admin can delete expenses
        if (mess.AdminMemberId != adminMemberId)
        {
            throw new UnauthorizedAccessException(
                "Only the mess admin can delete expenses.");
        }

        // 3. Find the expense
        var expense = await _context.Expenses
            .FirstOrDefaultAsync(e =>
                e.Id == expenseId &&
                e.MessId == messId);

        if (expense == null)
        {
            throw new InvalidOperationException(
                "Expense not found.");
        }

        // 4. Delete the expense
        _context.Expenses.Remove(expense);

        await _context.SaveChangesAsync();
    }


    // Get total cost for a mess
    public async Task<object> GetTotalCostAsync(
        int messId,
        int adminMemberId)
    {
        // 1. Check whether the mess exists
        var mess = await _context.Messes
            .FirstOrDefaultAsync(m => m.Id == messId);

        if (mess == null)
        {
            throw new InvalidOperationException(
                "Mess not found.");
        }

        // 2. Only the mess admin can view total cost
        if (mess.AdminMemberId != adminMemberId)
        {
            throw new UnauthorizedAccessException(
                "Only the mess admin can view total cost.");
        }

        // 3. Calculate totals by category
        var expenses = await _context.Expenses
            .Where(e => e.MessId == messId)
            .ToListAsync();

        var foodTotal = expenses
            .Where(e => e.Category == "Food")
            .Sum(e => e.Amount);

        var houseRentTotal = expenses
            .Where(e => e.Category == "HouseRent")
            .Sum(e => e.Amount);

        var chiefTotal = expenses
            .Where(e => e.Category == "Chief")
            .Sum(e => e.Amount);

        var othersTotal = expenses
            .Where(e => e.Category == "Others")
            .Sum(e => e.Amount);

        var memberCashTransferTotal = expenses
            .Where(e => e.Category == "MemberCashTransfer")
            .Sum(e => e.Amount);

        var grandTotal = expenses.Sum(e => e.Amount);

        return new
        {
            messId = messId,
            food = foodTotal,
            houseRent = houseRentTotal,
            chief = chiefTotal,
            others = othersTotal,
            memberCashTransfer = memberCashTransferTotal,
            grandTotal = grandTotal
        };
    }
}
