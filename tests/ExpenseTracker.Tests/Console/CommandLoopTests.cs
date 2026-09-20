using ExpenseTracker.ConsoleApp.Console;
using ExpenseTracker.Core.Ports;

namespace ExpenseTracker.Tests.Console;

public sealed class CommandLoopTests
{
    private static readonly DateOnly Today = new(2026, 9, 20);

    [Fact]
    public void List_WhenEmptyShowsClearMessage()
    {
        var (output, _) = Run("list\n");

        Assert.Contains("No expenses recorded.", output);
    }

    [Fact]
    public void AddAndList_RendersNormalizedExpense()
    {
        var (output, tracker) = Run("add\n12.50\n Food \n Lunch \n2026-01-02\nlist\n");

        Assert.Single(tracker.GetSnapshot());
        Assert.Contains("Expense added.", output);
        Assert.Contains("2026-01-02 | 12.50 | Food | Lunch", output);
    }

    [Fact]
    public void Add_WithOmittedDateUsesProviderDate()
    {
        var (_, tracker) = Run("add\n5\nTravel\n\n\n");

        Assert.Equal(Today, Assert.Single(tracker.GetSnapshot()).Date);
    }

    [Theory]
    [InlineData("not-a-number", "Food", "", "")]
    [InlineData("0", "Food", "", "")]
    [InlineData("1", "   ", "", "")]
    [InlineData("1", "Food", "", "09/20/2026")]
    public void InvalidAdd_RendersOneErrorAndContinues(
        string amount,
        string category,
        string description,
        string date)
    {
        var input = $"add\n{amount}\n{category}\n{description}\n{date}\nlist\n";

        var (output, tracker) = Run(input);

        Assert.Empty(tracker.GetSnapshot());
        Assert.Equal(1, Count(output, "Invalid input."));
        Assert.Contains("No expenses recorded.", output);
    }

    [Fact]
    public void UnknownCommand_RendersOneErrorAndContinuesWithoutChangingState()
    {
        var (output, tracker) = Run("total\nlist\n");

        Assert.Empty(tracker.GetSnapshot());
        Assert.Equal(1, Count(output, "Unknown command."));
        Assert.Contains("No expenses recorded.", output);
    }

    [Fact]
    public void EndOfInputDuringAdd_ExitsNormallyWithoutMutationOrError()
    {
        var (output, tracker) = Run("add\n10\n");

        Assert.Empty(tracker.GetSnapshot());
        Assert.DoesNotContain("Invalid input.", output);
    }

    [Fact]
    public void List_OrdersByDateThenInsertion()
    {
        var input = "add\n1\nsecond\n\n2026-02-01\n"
            + "add\n2\nfirst\n\n2026-01-01\n"
            + "add\n3\nthird\n\n2026-02-01\nlist\n";

        var (output, _) = Run(input);

        Assert.True(output.IndexOf("first", StringComparison.Ordinal) < output.IndexOf("second", StringComparison.Ordinal));
        Assert.True(output.IndexOf("second", StringComparison.Ordinal) < output.IndexOf("third", StringComparison.Ordinal));
    }

    private static (string Output, ExpenseTracker.Core.Services.ExpenseTracker Tracker) Run(string input)
    {
        var tracker = new ExpenseTracker.Core.Services.ExpenseTracker(new FakeDateProvider(Today));
        var output = new StringWriter();
        new CommandLoop(tracker).Run(new StringReader(input), output);
        return (output.ToString(), tracker);
    }

    private static int Count(string text, string value) =>
        text.Split(value, StringSplitOptions.None).Length - 1;

    private sealed record FakeDateProvider(DateOnly Today) : ILocalDateProvider;
}
