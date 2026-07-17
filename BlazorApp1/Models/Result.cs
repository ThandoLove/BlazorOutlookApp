using System;

namespace OperationalWorkspaceUI.Models;

/// <summary>
/// Phase 7 Long-Term Health Resolution: A unified outcome container 
/// to standardize success and failure response handling across all layers.
/// </summary>
public class Result<T> where T : class
{
    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public string ErrorMessage { get; private set; } = string.Empty;
    public string ErrorCode { get; private set; } = string.Empty;

    private Result(bool isSuccess, T? value, string errorMessage, string errorCode)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
    }

    public static Result<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Result<T>(true, value, string.Empty, string.Empty);
    }

    public static Result<T> Failure(string errorMessage, string errorCode = "GENERIC_ERROR")
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Failure result blocks must supply an actionable error message description.", nameof(errorMessage));

        return new Result<T>(false, null, errorMessage.Trim(), errorCode.Trim().ToUpperInvariant());
    }
}
