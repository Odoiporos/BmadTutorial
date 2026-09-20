using ExpenseTracker.Core.Ports;

namespace ExpenseTracker.ConsoleApp;

public sealed class SystemLocalDateProvider : ILocalDateProvider
{
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Now);
}
