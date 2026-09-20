namespace ExpenseTracker.Core.Results;

public enum OperationErrorCategory
{
    InvalidCommand,
    InvalidInput,
    StorageUnavailable,
    StorageDataInvalid
}

public sealed record OperationResult<T>
{
    private OperationResult(bool isSuccess, T? value, OperationErrorCategory? errorCategory)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorCategory = errorCategory;
    }

    public bool IsSuccess { get; }

    public T? Value { get; }

    public OperationErrorCategory? ErrorCategory { get; }

    public static OperationResult<T> Success(T value) => new(true, value, null);

    public static OperationResult<T> Failure(OperationErrorCategory category) => new(false, default, category);
}
