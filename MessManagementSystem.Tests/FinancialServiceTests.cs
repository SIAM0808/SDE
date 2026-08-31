using MessManagementSystem.Api.Data;
using MessManagementSystem.Api.Models;
using MessManagementSystem.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace MessManagementSystem.Tests;

public class FinancialServiceTests
{
private MessDbContext CreateContext()
{
var options = new DbContextOptionsBuilder<MessDbContext>()
.UseInMemoryDatabase(Guid.NewGuid().ToString())
.Options;

    return new MessDbContext(options);
}

private async Task SeedBasicData(MessDbContext context)
{
    var admin = new Member
    {
        Id = 1,
        Name = "Admin",
        Phone = "01700000001",
        Email = "admin@test.com",
        PasswordHash = "test",
        JoinDate = new DateTime(2026, 8, 1),
        IsActive = true,
        MessId = 1
    };

    var member = new Member
    {
        Id = 2,
        Name = "Test Member",
        Phone = "01700000002",
        Email = "member@test.com",
        PasswordHash = "test",
        JoinDate = new DateTime(2026, 8, 1),
        IsActive = true,
        MessId = 1
    };

    var mess = new Mess
    {
        Id = 1,
        Name = "Test Mess",
        MessCode = "TEST001",
        AdminMemberId = 1
    };

    context.Members.AddRange(admin, member);
    context.Messes.Add(mess);

    await context.SaveChangesAsync();
}

[Fact]
public async Task GetMemberFinancialSummaryAsync_CalculatesCorrectly()
{
    // Arrange
    using var context = CreateContext();

    await SeedBasicData(context);

    context.MemberPayments.Add(new MemberPayment
    {
        Id = 1,
        MessId = 1,
        MemberId = 2,
        Amount = 10000,
        PaymentDate = new DateTime(2026, 8, 10),
        RecordedBy = 1
    });

    context.Expenses.AddRange(
        new Expense
        {
            Id = 1,
            MessId = 1,
            Description = "House Rent",
            Category = "HouseRent",
            Amount = 4000,
            ExpenseDate = new DateTime(2026, 8, 1),
            RecordedBy = 1
        },
        new Expense
        {
            Id = 2,
            MessId = 1,
            Description = "Chief",
            Category = "Chief",
            Amount = 2000,
            ExpenseDate = new DateTime(2026, 8, 5),
            RecordedBy = 1
        },
        new Expense
        {
            Id = 3,
            MessId = 1,
            Description = "Others",
            Category = "Others",
            Amount = 1000,
            ExpenseDate = new DateTime(2026, 8, 10),
            RecordedBy = 1
        },
        new Expense
        {
            Id = 4,
            MessId = 1,
            Description = "Food",
            Category = "Food",
            Amount = 6000,
            ExpenseDate = new DateTime(2026, 8, 15),
            RecordedBy = 1
        }
    );

    context.Meals.AddRange(
        new Meal
        {
            Id = 1,
            MemberId = 2,
            MealDate = new DateTime(2026, 8, 10),
            Breakfast = 2,
            Lunch = 1,
            Dinner = 1
        },
        new Meal
        {
            Id = 2,
            MemberId = 1,
            MealDate = new DateTime(2026, 8, 10),
            Breakfast = 1,
            Lunch = 1,
            Dinner = 1
        }
    );

    await context.SaveChangesAsync();

    var service = new FinancialService(context);

    // Act
    var result = await service.GetMemberFinancialSummaryAsync(
        1,
        2,
        2026,
        8);

    // Assert
    Assert.Equal(10000, result.GivenMoney);

    // 4000 / 2 = 2000
    Assert.Equal(2000, result.HouseRent);

    // 2000 / 2 = 1000
    Assert.Equal(1000, result.ChiefBill);

    // 1000 / 2 = 500
    Assert.Equal(500, result.OthersBill);

    // Member meals = 2 + 1 + 1 = 4
    Assert.Equal(4, result.TotalMeals);

    // Total mess meals = 4 + 3 = 7
    // Meal rate = 6000 / 7
    Assert.Equal(6000m / 7m, result.MealRate);

    // Meal cost = 4 * meal rate
    Assert.Equal(4m * (6000m / 7m), result.MealCost);

    // Total expense
    Assert.Equal(
        2000m +
        1000m +
        500m +
        (4m * (6000m / 7m)),
        result.TotalExpense);

    // Due = Given Money - Total Expense
    Assert.Equal(
        10000m - result.TotalExpense,
        result.Due);

    Assert.True(result.CanOrderMeal);
    Assert.False(result.CanLeaveOrBeRemoved);
}


[Fact]
public async Task GetMemberFinancialSummaryAsync_WhenDueIsNegative_CannotOrderMeal()
{
    // Arrange
    using var context = CreateContext();

    await SeedBasicData(context);

    context.MemberPayments.Add(new MemberPayment
    {
        Id = 1,
        MessId = 1,
        MemberId = 2,
        Amount = 1000,
        PaymentDate = new DateTime(2026, 8, 10),
        RecordedBy = 1
    });

    context.Expenses.Add(new Expense
    {
        Id = 1,
        MessId = 1,
        Category = "HouseRent",
        Description = "House Rent",
        Amount = 4000,
        ExpenseDate = new DateTime(2026, 8, 1),
        RecordedBy = 1
    });

    await context.SaveChangesAsync();

    var service = new FinancialService(context);

    // Act
    var result = await service.GetMemberFinancialSummaryAsync(
        1,
        2,
        2026,
        8);

    // Assert
    Assert.True(result.Due < 0);
    Assert.False(result.CanOrderMeal);
    Assert.False(result.CanLeaveOrBeRemoved);
}


[Fact]
public async Task GetMemberFinancialSummaryAsync_WhenDueIsZero_CanLeaveOrBeRemoved()
{
    // Arrange
    using var context = CreateContext();

    await SeedBasicData(context);

    // Two members, therefore member's house rent share = 2000
    context.MemberPayments.Add(new MemberPayment
    {
        Id = 1,
        MessId = 1,
        MemberId = 2,
        Amount = 2000,
        PaymentDate = new DateTime(2026, 8, 10),
        RecordedBy = 1
    });

    context.Expenses.Add(new Expense
    {
        Id = 1,
        MessId = 1,
        Category = "HouseRent",
        Description = "House Rent",
        Amount = 4000,
        ExpenseDate = new DateTime(2026, 8, 1),
        RecordedBy = 1
    });

    await context.SaveChangesAsync();

    var service = new FinancialService(context);

    // Act
    var result = await service.GetMemberFinancialSummaryAsync(
        1,
        2,
        2026,
        8);

    // Assert
    Assert.Equal(0, result.Due);
    Assert.True(result.CanOrderMeal);
    Assert.True(result.CanLeaveOrBeRemoved);
}


[Fact]
public async Task GetMemberFinancialSummaryAsync_InvalidMonth_ThrowsException()
{
    // Arrange
    using var context = CreateContext();
    var service = new FinancialService(context);

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(
        async () =>
            await service.GetMemberFinancialSummaryAsync(
                1,
                2,
                2026,
                13));
}


[Fact]
public async Task GetMemberFinancialSummaryAsync_MemberNotFound_ThrowsException()
{
    // Arrange
    using var context = CreateContext();

    await SeedBasicData(context);

    var service = new FinancialService(context);

    // Act & Assert
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(
        async () =>
            await service.GetMemberFinancialSummaryAsync(
                1,
                999,
                2026,
                8));

    Assert.Equal(
        "Member not found in this mess.",
        exception.Message);
}


[Fact]
public async Task GetMemberBalanceAsync_CalculatesCorrectBalance()
{
    // Arrange
    using var context = CreateContext();

    await SeedBasicData(context);

    context.MemberPayments.Add(new MemberPayment
    {
        Id = 1,
        MessId = 1,
        MemberId = 2,
        Amount = 10000,
        PaymentDate = new DateTime(2026, 8, 10),
        RecordedBy = 1
    });

    context.Expenses.AddRange(
        new Expense
        {
            Id = 1,
            MessId = 1,
            Category = "HouseRent",
            Description = "House Rent",
            Amount = 4000,
            ExpenseDate = new DateTime(2026, 8, 1),
            RecordedBy = 1
        },
        new Expense
        {
            Id = 2,
            MessId = 1,
            Category = "Food",
            Description = "Food",
            Amount = 6000,
            ExpenseDate = new DateTime(2026, 8, 10),
            RecordedBy = 1
        }
    );

    context.Meals.AddRange(
        new Meal
        {
            Id = 1,
            MemberId = 2,
            MealDate = new DateTime(2026, 8, 10),
            Breakfast = 2,
            Lunch = 1,
            Dinner = 1
        },
        new Meal
        {
            Id = 2,
            MemberId = 1,
            MealDate = new DateTime(2026, 8, 10),
            Breakfast = 1,
            Lunch = 1,
            Dinner = 1
        }
    );

    await context.SaveChangesAsync();

    var service = new FinancialService(context);

    // Act
    var balance = await service.GetMemberBalanceAsync(1, 2);

    // Expected:
    // Given = 10000
    // House rent = 4000 / 2 = 2000
    // Total meals = 7
    // Member meals = 4
    // Meal rate = 6000 / 7
    // Meal cost = 4 * (6000 / 7)
    // Balance = Given - expenses

    var expected =
        10000m -
        2000m -
        (4m * (6000m / 7m));

    // Assert
    Assert.Equal(expected, balance);
}


}
