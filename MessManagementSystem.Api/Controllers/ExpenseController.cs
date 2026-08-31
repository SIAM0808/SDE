
using MessManagementSystem.Api.DTOs.Expense;
using MessManagementSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MessManagementSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpenseController : ControllerBase
{
    private readonly ExpenseService _expenseService;

    public ExpenseController(ExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    // Admin adds an expense
    [Authorize]
    [HttpPost("{messId}")]
    public async Task<IActionResult> CreateExpense(
        int messId,
        CreateExpenseRequest request)
    {
        var memberIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (memberIdClaim == null)
        {
            return Unauthorized();
        }

        var adminMemberId = int.Parse(memberIdClaim);

        try
        {
            var expense = await _expenseService.CreateExpenseAsync(
                messId,
                adminMemberId,
                request);

            return Ok(new
            {
                message = "Expense added successfully.",
                expense = new
                {
                    id = expense.Id,
                    messId = expense.MessId,
                    description = expense.Description,
                    category = expense.Category,
                    amount = expense.Amount,
                    expenseDate = expense.ExpenseDate,
                    recordedBy = expense.RecordedBy
                }
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }


    // Get all expenses for a mess
    [Authorize]
    [HttpGet("{messId}")]
    public async Task<IActionResult> GetExpenses(int messId)
    {
        var memberIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (memberIdClaim == null)
        {
            return Unauthorized();
        }

        var adminMemberId = int.Parse(memberIdClaim);

        try
        {
            var expenses =
                await _expenseService.GetExpensesAsync(
                    messId,
                    adminMemberId);

            return Ok(expenses);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }


    // Update an expense
    [Authorize]
    [HttpPut("{messId}/{expenseId}")]
    public async Task<IActionResult> UpdateExpense(
        int messId,
        int expenseId,
        UpdateExpenseRequest request)
    {
        var memberIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (memberIdClaim == null)
        {
            return Unauthorized();
        }

        var adminMemberId = int.Parse(memberIdClaim);

        try
        {
            var expense =
                await _expenseService.UpdateExpenseAsync(
                    messId,
                    adminMemberId,
                    expenseId,
                    request);

            return Ok(new
            {
                message = "Expense updated successfully.",
                expense = new
                {
                    id = expense.Id,
                    messId = expense.MessId,
                    description = expense.Description,
                    category = expense.Category,
                    amount = expense.Amount,
                    expenseDate = expense.ExpenseDate,
                    recordedBy = expense.RecordedBy
                }
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }


    // Delete an expense
    [Authorize]
    [HttpDelete("{messId}/{expenseId}")]
    public async Task<IActionResult> DeleteExpense(
        int messId,
        int expenseId)
    {
        var memberIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (memberIdClaim == null)
        {
            return Unauthorized();
        }

        var adminMemberId = int.Parse(memberIdClaim);

        try
        {
            await _expenseService.DeleteExpenseAsync(
                messId,
                adminMemberId,
                expenseId);

            return Ok(new
            {
                message = "Expense deleted successfully."
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }


    // Get total cost for a mess
    [Authorize]
    [HttpGet("{messId}/total")]
    public async Task<IActionResult> GetTotalCost(int messId)
    {
        var memberIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (memberIdClaim == null)
        {
            return Unauthorized();
        }

        var adminMemberId = int.Parse(memberIdClaim);

        try
        {
            var total =
                await _expenseService.GetTotalCostAsync(
                    messId,
                    adminMemberId);

            return Ok(total);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
}
