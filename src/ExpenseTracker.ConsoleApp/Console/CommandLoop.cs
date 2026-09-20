using System.Globalization;
using ExpenseTracker.Core.Results;
using ExpenseTracker.Core.Services;

namespace ExpenseTracker.ConsoleApp.Console;

public sealed class CommandLoop
{
    private readonly ExpenseTracker.Core.Services.ExpenseTracker _tracker;

    public CommandLoop(ExpenseTracker.Core.Services.ExpenseTracker tracker)
    {
        ArgumentNullException.ThrowIfNull(tracker);
        _tracker = tracker;
    }

    public void Run(TextReader input, TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);

        while (input.ReadLine() is { } command)
        {
            switch (command.Trim().ToLowerInvariant())
            {
                case "add":
                    if (!Add(input, output))
                    {
                        return;
                    }

                    break;
                case "list":
                    List(output);
                    break;
                default:
                    RenderError(output, OperationErrorCategory.InvalidCommand);
                    break;
            }
        }
    }

    private bool Add(TextReader input, TextWriter output)
    {
        output.Write("Amount: ");
        var amountText = input.ReadLine();
        if (amountText is null)
        {
            return false;
        }

        output.Write("Category: ");
        var category = input.ReadLine();
        if (category is null)
        {
            return false;
        }

        output.Write("Description (optional): ");
        var description = input.ReadLine();
        if (description is null)
        {
            return false;
        }

        output.Write("Date (yyyy-MM-dd, optional): ");
        var dateText = input.ReadLine();
        if (dateText is null)
        {
            return false;
        }

        if (!decimal.TryParse(amountText, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
        {
            RenderError(output, OperationErrorCategory.InvalidInput);
            return true;
        }

        DateOnly? date = null;
        if (!string.IsNullOrWhiteSpace(dateText))
        {
            if (!DateOnly.TryParseExact(
                    dateText,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsedDate))
            {
                RenderError(output, OperationErrorCategory.InvalidInput);
                return true;
            }

            date = parsedDate;
        }

        var result = _tracker.Add(amount, category, description, date);
        if (!result.IsSuccess)
        {
            RenderError(output, result.ErrorCategory!.Value);
        }
        else
        {
            output.WriteLine("Expense added.");
        }

        return true;
    }

    private void List(TextWriter output)
    {
        var expenses = _tracker.GetSnapshot();
        if (expenses.Count == 0)
        {
            output.WriteLine("No expenses recorded.");
            return;
        }

        foreach (var expense in expenses)
        {
            var description = expense.Description is null ? "-" : expense.Description;
            output.WriteLine(
                $"{expense.Date:yyyy-MM-dd} | {expense.Amount.ToString(CultureInfo.InvariantCulture)} | {expense.Category} | {description}");
        }
    }

    private static void RenderError(TextWriter output, OperationErrorCategory category)
    {
        var message = category switch
        {
            OperationErrorCategory.InvalidCommand => "Unknown command. Available commands: add, list.",
            OperationErrorCategory.InvalidInput => "Invalid input. Expense was not added.",
            OperationErrorCategory.StorageUnavailable => "Storage is unavailable.",
            OperationErrorCategory.StorageDataInvalid => "Stored expense data is invalid.",
            _ => "The operation could not be completed."
        };

        output.WriteLine(message);
    }
}
