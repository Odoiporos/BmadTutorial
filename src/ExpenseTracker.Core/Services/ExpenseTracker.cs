using System.Collections.ObjectModel;
using ExpenseTracker.Core.Models;
using ExpenseTracker.Core.Ports;
using ExpenseTracker.Core.Results;

namespace ExpenseTracker.Core.Services;

public sealed class ExpenseTracker
{
    private readonly ILocalDateProvider _localDateProvider;
    private readonly List<StoredExpense> _expenses = [];
    private long _nextSequence;

    public ExpenseTracker(ILocalDateProvider localDateProvider)
    {
        ArgumentNullException.ThrowIfNull(localDateProvider);
        _localDateProvider = localDateProvider;
    }

    public OperationResult<Expense> Add(
        decimal amount,
        string? category,
        string? description,
        DateOnly? date = null)
    {
        var result = Expense.Create(amount, category, description, date ?? _localDateProvider.Today);
        if (!result.IsSuccess)
        {
            return result;
        }

        var expense = result.Value!;
        _expenses.Add(new StoredExpense(expense, _nextSequence++));
        return OperationResult<Expense>.Success(expense);
    }

    public IReadOnlyList<Expense> GetSnapshot()
    {
        var snapshot = _expenses
            .OrderBy(stored => stored.Expense.Date)
            .ThenBy(stored => stored.Sequence)
            .Select(stored => stored.Expense)
            .ToArray();

        return new ReadOnlyCollection<Expense>(snapshot);
    }

    private sealed record StoredExpense(Expense Expense, long Sequence);
}
