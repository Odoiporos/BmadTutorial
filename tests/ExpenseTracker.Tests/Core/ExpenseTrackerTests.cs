using ExpenseTracker.Core.Ports;
using ExpenseTracker.Core.Results;

namespace ExpenseTracker.Tests.Core;

public sealed class ExpenseTrackerTests
{
    private static readonly DateOnly Today = new(2026, 9, 20);

    [Fact]
    public void Add_NormalizesValuesAndDefaultsDate()
    {
        var tracker = CreateTracker();

        var result = tracker.Add(12.50m, "  Groceries ", "  Lunch  ");

        Assert.True(result.IsSuccess);
        var expense = Assert.Single(tracker.GetSnapshot());
        Assert.Equal(12.50m, expense.Amount);
        Assert.Equal("Groceries", expense.Category);
        Assert.Equal("Lunch", expense.Description);
        Assert.Equal(Today, expense.Date);
    }

    [Fact]
    public void Add_NormalizesBlankDescriptionToNull()
    {
        var tracker = CreateTracker();

        tracker.Add(1m, "Food", "   ", new DateOnly(2026, 1, 2));

        Assert.Null(Assert.Single(tracker.GetSnapshot()).Description);
    }

    [Theory]
    [InlineData(0, "Food")]
    [InlineData(-1, "Food")]
    [InlineData(1, "")]
    [InlineData(1, "   ")]
    public void Add_InvalidValuesLeaveStateUnchanged(decimal amount, string category)
    {
        var tracker = CreateTracker();
        tracker.Add(3m, "Existing", null, Today);
        var before = tracker.GetSnapshot();

        var result = tracker.Add(amount, category, null, Today);

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationErrorCategory.InvalidInput, result.ErrorCategory);
        Assert.Equal(before, tracker.GetSnapshot());
    }

    [Fact]
    public void Snapshot_IsDetachedAndOrderedByDateThenInsertion()
    {
        var tracker = CreateTracker();
        tracker.Add(1m, "first-same-day", null, new DateOnly(2026, 2, 2));
        var earlierSnapshot = tracker.GetSnapshot();
        tracker.Add(2m, "earlier", null, new DateOnly(2026, 1, 1));
        tracker.Add(3m, "second-same-day", null, new DateOnly(2026, 2, 2));

        Assert.Single(earlierSnapshot);
        Assert.Equal(
            ["earlier", "first-same-day", "second-same-day"],
            tracker.GetSnapshot().Select(expense => expense.Category));
    }

    private static ExpenseTracker.Core.Services.ExpenseTracker CreateTracker() => new(new FakeDateProvider(Today));

    private sealed record FakeDateProvider(DateOnly Today) : ILocalDateProvider;
}
