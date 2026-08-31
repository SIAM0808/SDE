using MessManagementSystem.Api.Data;
using MessManagementSystem.Api.DTOs.Meal;
using MessManagementSystem.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MessManagementSystem.Api.Services;

public class MealService
{
    private readonly MessDbContext _context;
    private readonly FinancialService _financialService;

    public MealService(MessDbContext context, FinancialService financialService)
    {
        _context = context;
        _financialService = financialService;
    }

    public async Task<List<MealResponse>> OrderMealAsync(
        int memberId,
        OrderMealRequest request)
    {
        // 1. Check whether the member exists
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == memberId);

        if (member == null)
        {
            throw new InvalidOperationException(
                "Member not found.");
        }

        // 2. Member must belong to a mess
        if (member.MessId == null)
        {
            throw new InvalidOperationException(
                "You must belong to a mess to order meals.");
        }

        // 2b. Check member due (using current month, same as the financial summary UI)
        var now = DateTime.Now;
        var summary = await _financialService.GetMemberFinancialSummaryAsync(
            member.MessId.Value, memberId, now.Year, now.Month);
        if (summary.Due < 0)
        {
            throw new InvalidOperationException(
                "You cannot order a meal because you have a negative due.");
        }

        // 3. Validate negative quantities
        if (request.Breakfast < 0 ||
            request.Lunch < 0 ||
            request.Dinner < 0)
        {
            throw new InvalidOperationException(
                "Meal quantity cannot be negative.");
        }

        // 4. Maximum 10 for each meal type
        if (request.Breakfast > 10)
        {
            throw new InvalidOperationException(
                "You can order a maximum of 10 breakfasts at once.");
        }

        if (request.Lunch > 10)
        {
            throw new InvalidOperationException(
                "You can order a maximum of 10 lunches at once.");
        }

        if (request.Dinner > 10)
        {
            throw new InvalidOperationException(
                "You can order a maximum of 10 dinners at once.");
        }

        // 5. At least one meal must be ordered
        if (request.Breakfast == 0 &&
            request.Lunch == 0 &&
            request.Dinner == 0)
        {
            throw new InvalidOperationException(
                "You must order at least one meal.");
        }

        // 6. Current date and time
        var today = now.Date;

        // 7. Determine breakfast date
        //
        // Before 5:00 AM:
        //     today's breakfast
        //
        // At or after 5:00 AM:
        //     tomorrow's breakfast
        //
        DateTime? breakfastDate = null;

        if (request.Breakfast > 0)
        {
            breakfastDate =
                now.TimeOfDay < TimeSpan.FromHours(5)
                    ? today
                    : today.AddDays(1);
        }

        // 8. Validate lunch deadline
        if (request.Lunch > 0 &&
            now.TimeOfDay >= TimeSpan.FromHours(11))
        {
            throw new InvalidOperationException(
                "Lunch can only be ordered before 11:00 AM.");
        }

        // 9. Validate dinner deadline
        if (request.Dinner > 0 &&
            now.TimeOfDay >= TimeSpan.FromHours(20))
        {
            throw new InvalidOperationException(
                "Dinner can only be ordered before 8:00 PM.");
        }

        // 10. Determine which dates are affected
        var mealDates = new List<DateTime>();

        if (breakfastDate.HasValue)
        {
            mealDates.Add(breakfastDate.Value);
        }

        if (request.Lunch > 0 ||
            request.Dinner > 0)
        {
            if (!mealDates.Contains(today))
            {
                mealDates.Add(today);
            }
        }

        // 11. Get existing meal records
        var meals = await _context.Meals
            .Where(m =>
                m.MemberId == memberId &&
                mealDates.Contains(m.MealDate))
            .ToListAsync();

        // 12. Validate existing + new breakfast quantity
        if (request.Breakfast > 0)
        {
            var breakfastMeal = meals
                .FirstOrDefault(m =>
                    m.MealDate == breakfastDate!.Value);

            var existingBreakfast =
                breakfastMeal?.Breakfast ?? 0;

            if (existingBreakfast + request.Breakfast > 10)
            {
                throw new InvalidOperationException(
                    "Total breakfast meals for this date cannot exceed 10.");
            }
        }

        // 13. Validate existing + new lunch quantity
        if (request.Lunch > 0)
        {
            var lunchMeal = meals
                .FirstOrDefault(m =>
                    m.MealDate == today);

            var existingLunch =
                lunchMeal?.Lunch ?? 0;

            if (existingLunch + request.Lunch > 10)
            {
                throw new InvalidOperationException(
                    "Total lunch meals for this date cannot exceed 10.");
            }
        }

        // 14. Validate existing + new dinner quantity
        if (request.Dinner > 0)
        {
            var dinnerMeal = meals
                .FirstOrDefault(m =>
                    m.MealDate == today);

            var existingDinner =
                dinnerMeal?.Dinner ?? 0;

            if (existingDinner + request.Dinner > 10)
            {
                throw new InvalidOperationException(
                    "Total dinner meals for this date cannot exceed 10.");
            }
        }

        // 15. Add/update breakfast
        if (request.Breakfast > 0)
        {
            var meal = meals
                .FirstOrDefault(m =>
                    m.MealDate == breakfastDate!.Value);

            if (meal == null)
            {
                meal = new Meal
                {
                    MemberId = memberId,
                    MealDate = breakfastDate!.Value,
                    Breakfast = request.Breakfast,
                    Lunch = 0,
                    Dinner = 0
                };

                _context.Meals.Add(meal);
                meals.Add(meal);
            }
            else
            {
                meal.Breakfast += request.Breakfast;
            }
        }

        // 16. Add/update lunch
        if (request.Lunch > 0)
        {
            var meal = meals
                .FirstOrDefault(m =>
                    m.MealDate == today);

            if (meal == null)
            {
                meal = new Meal
                {
                    MemberId = memberId,
                    MealDate = today,
                    Breakfast = 0,
                    Lunch = request.Lunch,
                    Dinner = 0
                };

                _context.Meals.Add(meal);
                meals.Add(meal);
            }
            else
            {
                meal.Lunch += request.Lunch;
            }
        }

        // 17. Add/update dinner
        if (request.Dinner > 0)
        {
            var meal = meals
                .FirstOrDefault(m =>
                    m.MealDate == today);

            if (meal == null)
            {
                meal = new Meal
                {
                    MemberId = memberId,
                    MealDate = today,
                    Breakfast = 0,
                    Lunch = 0,
                    Dinner = request.Dinner
                };

                _context.Meals.Add(meal);
                meals.Add(meal);
            }
            else
            {
                meal.Dinner += request.Dinner;
            }
        }

        // 18. Save everything together
        await _context.SaveChangesAsync();

        // 19. Return affected meal records
        return meals
            .Where(m => mealDates.Contains(m.MealDate))
            .OrderBy(m => m.MealDate)
            .Select(m => new MealResponse
            {
                Id = m.Id,
                MemberId = m.MemberId,
                MealDate = m.MealDate,
                Breakfast = m.Breakfast,
                Lunch = m.Lunch,
                Dinner = m.Dinner
            })
            .ToList();
    }


    // Get all meals for a member
    public async Task<List<MealResponse>> GetMyMealsAsync(
        int memberId)
    {
        // 1. Check whether the member exists
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == memberId);

        if (member == null)
        {
            throw new InvalidOperationException(
                "Member not found.");
        }

        // 2. Member must belong to a mess
        if (member.MessId == null)
        {
            throw new InvalidOperationException(
                "You must belong to a mess to view meals.");
        }

        // 3. Get the member's meals
        var meals = await _context.Meals
            .Where(m => m.MemberId == memberId)
            .OrderByDescending(m => m.MealDate)
            .Select(m => new MealResponse
            {
                Id = m.Id,
                MemberId = m.MemberId,
                MealDate = m.MealDate,
                Breakfast = m.Breakfast,
                Lunch = m.Lunch,
                Dinner = m.Dinner
            })
            .ToListAsync();

        return meals;
    }


    // Update a meal record
    public async Task<MealResponse> UpdateMealAsync(
        int memberId,
        int mealId,
        UpdateMealRequest request)
    {
        // 1. Check whether the member exists
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == memberId);

        if (member == null)
        {
            throw new InvalidOperationException(
                "Member not found.");
        }

        // 2. Member must belong to a mess
        if (member.MessId == null)
        {
            throw new InvalidOperationException(
                "You must belong to a mess to update meals.");
        }

        // 3. Find the meal record
        var meal = await _context.Meals
            .FirstOrDefaultAsync(m =>
                m.Id == mealId &&
                m.MemberId == memberId);

        if (meal == null)
        {
            throw new InvalidOperationException(
                "Meal record not found.");
        }

        // 3b. Check member due for meal increases (using current month, same as financial summary UI)
        var now = DateTime.Now;
        var summary = await _financialService.GetMemberFinancialSummaryAsync(
            member.MessId.Value, memberId, now.Year, now.Month);
        var oldTotal = meal.Breakfast + meal.Lunch + meal.Dinner;
        var newTotal = request.Breakfast + request.Lunch + request.Dinner;
        if (summary.Due < 0 && newTotal > oldTotal)
        {
            throw new InvalidOperationException(
                "You cannot increase your meal order because you have a negative due.");
        }

        // 4. Validate quantities
        if (request.Breakfast < 0 ||
            request.Lunch < 0 ||
            request.Dinner < 0)
        {
            throw new InvalidOperationException(
                "Meal quantity cannot be negative.");
        }

        if (request.Breakfast > 10)
        {
            throw new InvalidOperationException(
                "Breakfast cannot exceed 10.");
        }

        if (request.Lunch > 10)
        {
            throw new InvalidOperationException(
                "Lunch cannot exceed 10.");
        }

        if (request.Dinner > 10)
        {
            throw new InvalidOperationException(
                "Dinner cannot exceed 10.");
        }

        // 5. Update the meal
        meal.Breakfast = request.Breakfast;
        meal.Lunch = request.Lunch;
        meal.Dinner = request.Dinner;

        await _context.SaveChangesAsync();

        return new MealResponse
        {
            Id = meal.Id,
            MemberId = meal.MemberId,
            MealDate = meal.MealDate,
            Breakfast = meal.Breakfast,
            Lunch = meal.Lunch,
            Dinner = meal.Dinner
        };
    }


    // Delete a meal record
    public async Task DeleteMealAsync(
        int memberId,
        int mealId)
    {
        // 1. Check whether the member exists
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == memberId);

        if (member == null)
        {
            throw new InvalidOperationException(
                "Member not found.");
        }

        // 2. Find the meal record
        var meal = await _context.Meals
            .FirstOrDefaultAsync(m =>
                m.Id == mealId &&
                m.MemberId == memberId);

        if (meal == null)
        {
            throw new InvalidOperationException(
                "Meal record not found.");
        }

        // 3. Delete the meal
        _context.Meals.Remove(meal);

        await _context.SaveChangesAsync();
    }


    // Get meal totals for a member
    public async Task<object> GetMealTotalsAsync(int memberId)
    {
        // 1. Check whether the member exists
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == memberId);

        if (member == null)
        {
            throw new InvalidOperationException(
                "Member not found.");
        }

        // 2. Member must belong to a mess
        if (member.MessId == null)
        {
            throw new InvalidOperationException(
                "You must belong to a mess to view meal totals.");
        }

        // 3. Sum all meals
        var meals = await _context.Meals
            .Where(m => m.MemberId == memberId)
            .ToListAsync();

        var totalBreakfast = meals.Sum(m => m.Breakfast);
        var totalLunch = meals.Sum(m => m.Lunch);
        var totalDinner = meals.Sum(m => m.Dinner);

        return new
        {
            memberId = memberId,
            totalBreakfast = totalBreakfast,
            totalLunch = totalLunch,
            totalDinner = totalDinner,
            grandTotal = totalBreakfast + totalLunch + totalDinner
        };
    }


    // Get meal totals for the entire mess
    public async Task<object> GetMessMealTotalsAsync(int messId)
    {
        // 1. Check whether the mess exists
        var mess = await _context.Messes
            .FirstOrDefaultAsync(m => m.Id == messId);

        if (mess == null)
        {
            throw new InvalidOperationException(
                "Mess not found.");
        }

        // 2. Sum all meals for members in this mess
        var meals = await _context.Meals
            .Where(m => m.Member.MessId == messId)
            .ToListAsync();

        var totalBreakfast = meals.Sum(m => m.Breakfast);
        var totalLunch = meals.Sum(m => m.Lunch);
        var totalDinner = meals.Sum(m => m.Dinner);

        return new
        {
            messId = messId,
            totalBreakfast = totalBreakfast,
            totalLunch = totalLunch,
            totalDinner = totalDinner,
            grandTotal = totalBreakfast + totalLunch + totalDinner
        };
    }


}

