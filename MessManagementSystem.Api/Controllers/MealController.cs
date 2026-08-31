using MessManagementSystem.Api.DTOs.Meal;
using MessManagementSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MessManagementSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MealController : ControllerBase
{
    private readonly MealService _mealService;

    public MealController(MealService mealService)
    {
        _mealService = mealService;
    }

    // Order meals
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> OrderMeal(
        OrderMealRequest request)
    {
        var memberIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (memberIdClaim == null)
        {
            return Unauthorized();
        }

        var memberId = int.Parse(memberIdClaim);

        try
        {
            var meals = await _mealService.OrderMealAsync(
                memberId,
                request);

            return Ok(new
            {
                message = "Meal ordered successfully.",
                meals = meals
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


    // View my meals
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetMyMeals()
    {
        var memberIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (memberIdClaim == null)
        {
            return Unauthorized();
        }

        var memberId = int.Parse(memberIdClaim);

        try
        {
            var meals = await _mealService.GetMyMealsAsync(
                memberId);

            return Ok(meals);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }


    // Update a meal record
    [Authorize]
    [HttpPut("{mealId}")]
    public async Task<IActionResult> UpdateMeal(
        int mealId,
        UpdateMealRequest request)
    {
        var memberIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (memberIdClaim == null)
        {
            return Unauthorized();
        }

        var memberId = int.Parse(memberIdClaim);

        try
        {
            var meal = await _mealService.UpdateMealAsync(
                memberId,
                mealId,
                request);

            return Ok(new
            {
                message = "Meal updated successfully.",
                meal = meal
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


    // Delete a meal record
    [Authorize]
    [HttpDelete("{mealId}")]
    public async Task<IActionResult> DeleteMeal(int mealId)
    {
        var memberIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (memberIdClaim == null)
        {
            return Unauthorized();
        }

        var memberId = int.Parse(memberIdClaim);

        try
        {
            await _mealService.DeleteMealAsync(
                memberId,
                mealId);

            return Ok(new
            {
                message = "Meal deleted successfully."
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


    // Get my meal totals
    [Authorize]
    [HttpGet("my-totals")]
    public async Task<IActionResult> GetMyMealTotals()
    {
        var memberIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (memberIdClaim == null)
        {
            return Unauthorized();
        }

        var memberId = int.Parse(memberIdClaim);

        try
        {
            var totals = await _mealService.GetMealTotalsAsync(
                memberId);

            return Ok(totals);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }


    // Get mess meal totals
    [Authorize]
    [HttpGet("mess-totals/{messId}")]
    public async Task<IActionResult> GetMessMealTotals(
        int messId)
    {
        try
        {
            var totals =
                await _mealService.GetMessMealTotalsAsync(
                    messId);

            return Ok(totals);
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
