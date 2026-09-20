using ExpenseTracker.Core.Results;

namespace ExpenseTracker.Core.Models;

public sealed record Expense
{
    private Expense(decimal amount, string category, string? description, DateOnly date)
    {
        Amount = amount;
        Category = category;
        Description = description;
        Date = date;
    }

    public decimal Amount { get; }

    public string Category { get; }

    public string? Description { get; }

    public DateOnly Date { get; }

    public static OperationResult<Expense> Create(
        decimal amount,
        string? category,
        string? description,
        DateOnly date)
    {
        var normalizedCategory = category?.Trim();
        if (amount <= 0 || string.IsNullOrWhiteSpace(normalizedCategory))
        {
            return OperationResult<Expense>.Failure(OperationErrorCategory.InvalidInput);
        }

        var normalizedDescription = description?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedDescription))
        {
            normalizedDescription = null;
        }

        return OperationResult<Expense>.Success(
            new Expense(amount, normalizedCategory, normalizedDescription, date));
    }
}
