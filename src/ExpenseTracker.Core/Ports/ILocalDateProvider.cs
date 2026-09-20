namespace ExpenseTracker.Core.Ports;

public interface ILocalDateProvider
{
    DateOnly Today { get; }
}
