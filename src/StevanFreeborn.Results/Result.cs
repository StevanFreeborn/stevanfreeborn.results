namespace StevanFreeborn.Results
{

  /// <summary>
  /// Static factory methods for creating result instances.
  /// </summary>
  public static class Result
  {
    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="value">The value.</param>
    /// <returns>A successful <see cref="Result{T, TError}"/>.</returns>
    public static Result<T, TError> Ok<T, TError>(T value) where TError : IError
    {
      return new Result<T, TError>(true, value, default);
    }

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="error">The error.</param>
    /// <returns>A failed <see cref="Result{T, TError}"/>.</returns>
    public static Result<T, TError> Fail<T, TError>(TError error) where TError : IError
    {
      return new Result<T, TError>(false, default!, error);
    }

    /// <summary>
    /// Executes the specified function and wraps the result in a <see cref="Result{T, TError}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>A successful result with the return value if no exception is thrown; otherwise, a failed result.</returns>
    public static Result<T, TError> Try<T, TError>(Func<T> func) where TError : IError
    {
      return Try(func, ex => (TError)(IError)new Error("UnexpectedError", ex.Message));
    }

    /// <summary>
    /// Executes the specified function and wraps the result in a <see cref="Result{T, TError}"/> using the specified error handler.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <param name="errorHandler">The function to convert exceptions to errors.</param>
    /// <returns>A successful result with the return value if no exception is thrown; otherwise, a failed result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when func or errorHandler is null.</exception>
    public static Result<T, TError> Try<T, TError>(Func<T> func, Func<Exception, TError> errorHandler) where TError : IError
    {
      if (func is null)
      {
        throw new ArgumentNullException(nameof(func));
      }

      if (errorHandler is null)
      {
        throw new ArgumentNullException(nameof(errorHandler));
      }

      try
      {
        return Ok<T, TError>(func());
      }
      catch (Exception ex)
      {
        return Fail<T, TError>(errorHandler(ex));
      }
    }
  }

  /// <summary>
  /// Represents a result with a value and error type, used for operations that either succeed with a value or fail with an error.
  /// </summary>
  /// <typeparam name="T">The type of the value.</typeparam>
  /// <typeparam name="TError">The type of the error.</typeparam>
  public sealed class Result<T, TError> where TError : IError
  {
    /// <summary>
    /// Gets a value indicating whether the result is successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the result is a failure.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the value of the result.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing Value on a failed result.</exception>
    public T Value => IsSuccess
        ? _value
        : throw new InvalidOperationException($"Cannot access Value on a failed result. Error: [{Error.Code}] {Error.Message}");

    /// <summary>
    /// Gets the error associated with the result.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing Error on a successful result.</exception>
    public TError Error => _error ?? throw new InvalidOperationException("Cannot access Error on a successful result.");

    private readonly T _value;
    private readonly TError? _error;

    internal Result(bool isSuccess, T value, TError? error)
    {
      IsSuccess = isSuccess;
      _value = value;
      _error = error;
    }

    /// <summary>
    /// Implicitly converts a value to a successful <see cref="Result{T, TError}"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator Result<T, TError>(T value)
    {
      return Result.Ok<T, TError>(value);
    }

    /// <summary>
    /// Implicitly converts an <see cref="IError"/> to a failed <see cref="Result{T, TError}"/>.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    public static implicit operator Result<T, TError>(TError error)
    {
      return Result.Fail<T, TError>(error);
    }

    /// <summary>
    /// Maps the value to a new type if the result is successful.
    /// </summary>
    /// <typeparam name="TNew">The new type.</typeparam>
    /// <param name="mapper">The function to map the value.</param>
    /// <returns>A new <see cref="Result{TNew, TError}"/> with the mapped value if successful, otherwise the current error.</returns>
    /// <exception cref="ArgumentNullException">Thrown when mapper is null.</exception>
    public Result<TNew, TError> Map<TNew>(Func<T, TNew> mapper)
    {
      if (mapper is null)
      {
        throw new ArgumentNullException(nameof(mapper));
      }

      return IsSuccess ? Result.Ok<TNew, TError>(mapper(Value)) : Result.Fail<TNew, TError>(Error);
    }

    /// <summary>
    /// Maps the error to a new error if the result is a failure.
    /// </summary>
    /// <param name="mapper">The function to map the error.</param>
    /// <returns>A new <see cref="Result{T, TNewError}"/> with the mapped error if failed, otherwise the current result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when mapper is null.</exception>
    public Result<T, TNewError> MapError<TNewError>(Func<TError, TNewError> mapper) where TNewError : IError
    {
      if (mapper is null)
      {
        throw new ArgumentNullException(nameof(mapper));
      }

      return IsFailure ? Result.Fail<T, TNewError>(mapper(Error)) : Result.Ok<T, TNewError>(Value);
    }

    /// <summary>
    /// Binds to a new result if the current result is successful.
    /// </summary>
    /// <typeparam name="TNew">The new result type.</typeparam>
    /// <param name="binder">The function to bind to on success.</param>
    /// <returns>The result of the binder function if successful, otherwise the current error.</returns>
    /// <exception cref="ArgumentNullException">Thrown when binder is null.</exception>
    public Result<TNew, TError> Bind<TNew>(Func<T, Result<TNew, TError>> binder)
    {
      if (binder is null)
      {
        throw new ArgumentNullException(nameof(binder));
      }

      return IsSuccess ? binder(Value) : Result.Fail<TNew, TError>(Error);
    }

    /// <summary>
    /// Matches the result and returns a value based on success or failure.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="onSuccess">The function to execute on success.</param>
    /// <param name="onFailure">The function to execute on failure.</param>
    /// <returns>The result of the appropriate function.</returns>
    /// <exception cref="ArgumentNullException">Thrown when onSuccess or onFailure is null.</exception>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<TError, TResult> onFailure)
    {
      if (onSuccess is null)
      {
        throw new ArgumentNullException(nameof(onSuccess));
      }

      if (onFailure is null)
      {
        throw new ArgumentNullException(nameof(onFailure));
      }

      return IsSuccess ? onSuccess(Value) : onFailure(Error);
    }

    /// <summary>
    /// Matches the result and executes the appropriate action.
    /// </summary>
    /// <param name="onSuccess">The action to execute on success.</param>
    /// <param name="onFailure">The action to execute on failure.</param>
    /// <exception cref="ArgumentNullException">Thrown when onSuccess or onFailure is null.</exception>
    public void Match(Action<T> onSuccess, Action<TError> onFailure)
    {
      if (onSuccess is null)
      {
        throw new ArgumentNullException(nameof(onSuccess));
      }

      if (onFailure is null)
      {
        throw new ArgumentNullException(nameof(onFailure));
      }

      if (IsSuccess)
      {
        onSuccess(Value);
      }
      else
      {
        onFailure(Error);
      }
    }

    /// <summary>
    /// Executes the specified action if the result is successful, and returns the current result.
    /// </summary>
    /// <param name="action">The action to execute on success.</param>
    /// <returns>The current result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
    public Result<T, TError> Map(Action<T> action)
    {
      if (action is null)
      {
        throw new ArgumentNullException(nameof(action));
      }

      if (IsSuccess)
      {
        action(Value);
      }
      return this;
    }

    /// <summary>
    /// Executes the specified action if the result is successful, and returns the current result.
    /// </summary>
    /// <param name="action">The action to execute on success.</param>
    /// <returns>The current result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
    public Result<T, TError> Map(Action action)
    {
      if (action is null)
      {
        throw new ArgumentNullException(nameof(action));
      }

      if (IsSuccess)
      {
        action();
      }
      return this;
    }
  }

}