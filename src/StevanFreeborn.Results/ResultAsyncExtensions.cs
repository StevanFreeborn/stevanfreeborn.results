namespace StevanFreeborn.Results
{

  /// <summary>
  /// Provides async extension methods for <see cref="Result{Unit, Error}"/> and <see cref="Result{T, Error}"/>.
  /// </summary>
  public static class ResultAsyncExtensions
  {
    /// <summary>
    /// Maps the value to a new type asynchronously if the result is successful.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TNew">The new type.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="mapper">The async function to map the value.</param>
    /// <returns>A task containing a new <see cref="Result{TNew, TError}"/> with the mapped value if successful, otherwise the current error.</returns>
    /// <exception cref="ArgumentNullException">Thrown when result or mapper is null.</exception>
    public static async Task<Result<TNew, TError>> MapAsync<T, TNew, TError>(this Result<T, TError> result, Func<T, Task<TNew>> mapper)
        where TError : IError
    {
      #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(result);
      #else
        if (result is null)
        {
          throw new ArgumentNullException(nameof(result));
        }
      #endif

      #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(mapper);
      #else
        if (mapper is null)
        {
          throw new ArgumentNullException(nameof(mapper));
        }
      #endif

      return result.IsSuccess ? await mapper(result.Value).ConfigureAwait(false) : result.Error;
    }

    /// <summary>
    /// Maps the error to a new error asynchronously if the result is a failure.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <typeparam name="TNewError">The new error type.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="mapper">The async function to map the error.</param>
    /// <returns>A task containing a new <see cref="Result{T, TNewError}"/> with the mapped error if failed, otherwise the current result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when result or mapper is null.</exception>
    public static async Task<Result<T, TNewError>> MapErrorAsync<T, TError, TNewError>(this Result<T, TError> result, Func<TError, Task<TNewError>> mapper)
        where TError : IError
        where TNewError : IError
    {
      #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(result);
      #else
        if (result is null)
        {
          throw new ArgumentNullException(nameof(result));
        }
      #endif

      #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(mapper);
      #else
        if (mapper is null)
        {
          throw new ArgumentNullException(nameof(mapper));
        }
      #endif

      return result.IsFailure ? await mapper(result.Error).ConfigureAwait(false) : Result.Ok<T, TNewError>(result.Value);
    }

    /// <summary>
    /// Executes the specified async action if the result is successful, and returns the current result.
    /// </summary>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="onSuccess">The async action to execute on success.</param>
    /// <returns>A task containing the result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when result or onSuccess is null.</exception>
    public static async Task<Result<Unit, TError>> MapAsync<TError>(this Result<Unit, TError> result, Func<Unit, Task> onSuccess)
        where TError : IError
    {
      #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(result);
      #else
        if (result is null)
        {
          throw new ArgumentNullException(nameof(result));
        }
      #endif

      #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(onSuccess);
      #else
        if (onSuccess is null)
        {
          throw new ArgumentNullException(nameof(onSuccess));
        }
      #endif

      if (result.IsSuccess)
      {
        await onSuccess(result.Value).ConfigureAwait(false);
      }

      return result;
    }

    /// <summary>
    /// Binds to a new async result if the current result is successful.
    /// </summary>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="onSuccess">The async function to execute and bind to on success.</param>
    /// <returns>A task containing the result of the binder function if successful, otherwise the current result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when result or onSuccess is null.</exception>
    public static async Task<Result<Unit, TError>> BindAsync<TError>(this Result<Unit, TError> result, Func<Unit, Task<Result<Unit, TError>>> onSuccess)
        where TError : IError
    {
      #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(result);
      #else
        if (result is null)
        {
          throw new ArgumentNullException(nameof(result));
        }
      #endif

      #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(onSuccess);
      #else
        if (onSuccess is null)
        {
          throw new ArgumentNullException(nameof(onSuccess));
        }
      #endif

      return result.IsSuccess ? await onSuccess(result.Value).ConfigureAwait(false) : result;
    }

    /// <summary>
    /// Matches the result asynchronously and returns a value based on success or failure.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="onSuccess">The async function to execute on success.</param>
    /// <param name="onFailure">The async function to execute on failure.</param>
    /// <returns>A task containing the result of the appropriate function.</returns>
    /// <exception cref="ArgumentNullException">Thrown when result, onSuccess, or onFailure is null.</exception>
    public static async Task<TResult> MatchAsync<TResult, TError>(this Result<Unit, TError> result, Func<Unit, Task<TResult>> onSuccess, Func<TError, Task<TResult>> onFailure)
        where TError : IError
    {
      #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(result);
      #else
        if (result is null)
        {
          throw new ArgumentNullException(nameof(result));
        }
      #endif

      #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(onSuccess);
      #else
        if (onSuccess is null)
        {
          throw new ArgumentNullException(nameof(onSuccess));
        }
      #endif

      #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(onFailure);
      #else
        if (onFailure is null)
        {
          throw new ArgumentNullException(nameof(onFailure));
        }
      #endif

      return result.IsSuccess
          ? await onSuccess(result.Value).ConfigureAwait(false)
          : await onFailure(result.Error).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the specified async action and wraps the result in a <see cref="Result{Unit, Error}"/>.
    /// </summary>
    /// <param name="action">The async action to execute.</param>
    /// <returns>A task containing a successful result if no exception is thrown; otherwise, a failed result.</returns>
    public static Task<Result<Unit, Error>> TryAsync(Func<Task> action)
    {
      return TryAsync(action, ex => new Error("UnexpectedError", ex.Message));
    }

    /// <summary>
    /// Executes the specified async action and wraps the result in a <see cref="Result{Unit, Error}"/> using the specified error handler.
    /// </summary>
    /// <param name="action">The async action to execute.</param>
    /// <param name="errorHandler">The function to convert exceptions to errors.</param>
    /// <returns>A task containing a successful result if no exception is thrown; otherwise, a failed result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when action or errorHandler is null.</exception>
    public static async Task<Result<Unit, Error>> TryAsync(Func<Task> action, Func<Exception, Error> errorHandler)
    {
      #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(action);
      #else
        if (action is null)
        {
          throw new ArgumentNullException(nameof(action));
        }
      #endif

      #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(errorHandler);
      #else
        if (errorHandler is null)
        {
          throw new ArgumentNullException(nameof(errorHandler));
        }
      #endif

      try
      {
        await action().ConfigureAwait(false);
        return Result.Ok<Unit, Error>(default);
      }
      catch (Exception ex)
      {
        return Result.Fail<Unit, Error>(errorHandler(ex));
      }
    }

    /// <summary>
    /// Binds to a new async result if the current result is successful.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TNew">The new result type.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="binder">The async function to bind to on success.</param>
    /// <returns>A task containing the result of the binder function if successful, otherwise the current error.</returns>
    /// <exception cref="ArgumentNullException">Thrown when result or binder is null.</exception>
    public static async Task<Result<TNew, TError>> BindAsync<T, TNew, TError>(this Result<T, TError> result, Func<T, Task<Result<TNew, TError>>> binder)
        where TError : IError
    {
      #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(result);
      #else
        if (result is null)
        {
          throw new ArgumentNullException(nameof(result));
        }
      #endif

      #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(binder);
      #else
        if (binder is null)
        {
          throw new ArgumentNullException(nameof(binder));
        }
      #endif

      return result.IsSuccess ? await binder(result.Value).ConfigureAwait(false) : Result.Fail<TNew, TError>(result.Error);
    }

    /// <summary>
    /// Matches the result asynchronously and returns a value based on success or failure.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="onSuccess">The async function to execute on success.</param>
    /// <param name="onFailure">The async function to execute on failure.</param>
    /// <returns>A task containing the result of the appropriate function.</returns>
    /// <exception cref="ArgumentNullException">Thrown when result, onSuccess, or onFailure is null.</exception>
    public static async Task<TResult> MatchAsync<T, TResult, TError>(this Result<T, TError> result, Func<T, Task<TResult>> onSuccess, Func<TError, Task<TResult>> onFailure)
        where TError : IError
    {
      #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(result);
      #else
        if (result is null)
        {
          throw new ArgumentNullException(nameof(result));
        }
      #endif

      #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(onSuccess);
      #else
        if (onSuccess is null)
        {
          throw new ArgumentNullException(nameof(onSuccess));
        }
      #endif

      #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(onFailure);
      #else
        if (onFailure is null)
        {
          throw new ArgumentNullException(nameof(onFailure));
        }
      #endif

      return result.IsSuccess
          ? await onSuccess(result.Value).ConfigureAwait(false)
          : await onFailure(result.Error).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the specified async function and wraps the result in a <see cref="Result{T, Error}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="func">The async function to execute.</param>
    /// <returns>A task containing a successful result with the return value if no exception is thrown; otherwise, a failed result.</returns>
    public static Task<Result<T, Error>> TryAsync<T>(Func<Task<T>> func)
    {
      return TryAsync(func, ex => new Error("UnexpectedError", ex.Message));
    }

    /// <summary>
    /// Executes the specified async function and wraps the result in a <see cref="Result{T, Error}"/> using the specified error handler.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="func">The async function to execute.</param>
    /// <param name="errorHandler">The function to convert exceptions to errors.</param>
    /// <returns>A task containing a successful result with the return value if no exception is thrown; otherwise, a failed result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when func or errorHandler is null.</exception>
    public static async Task<Result<T, Error>> TryAsync<T>(Func<Task<T>> func, Func<Exception, Error> errorHandler)
    {
      #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(func);
      #else
        if (func is null)
        {
          throw new ArgumentNullException(nameof(func));
        }
      #endif

      #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(errorHandler);
      #else
        if (errorHandler is null)
        {
          throw new ArgumentNullException(nameof(errorHandler));
        }
      #endif

      try
      {
        return Result.Ok<T, Error>(await func().ConfigureAwait(false));
      }
      catch (Exception ex)
      {
        return Result.Fail<T, Error>(errorHandler(ex));
      }
    }
  }

}