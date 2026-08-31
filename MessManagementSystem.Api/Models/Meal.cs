namespace MessManagementSystem.Api.Models;

public class Meal
{
    public int Id { get; set; }

    public int MemberId { get; set; }

    public Member? Member { get; set; }

    // The date on which the meal will be consumed
    public DateTime MealDate { get; set; }

    public int Breakfast { get; set; }

    public int Lunch { get; set; }

    public int Dinner { get; set; }
}