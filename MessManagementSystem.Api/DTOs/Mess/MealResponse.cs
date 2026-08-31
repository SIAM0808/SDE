namespace MessManagementSystem.Api.DTOs.Meal;

public class MealResponse
{
    public int Id { get; set; }

    public int MemberId { get; set; }

    public DateTime MealDate { get; set; }

    public int Breakfast { get; set; }

    public int Lunch { get; set; }

    public int Dinner { get; set; }

    public int Total =>
        Breakfast + Lunch + Dinner;
}
