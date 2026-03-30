# StevanFreeborn.Results

[![NuGet](https://img.shields.io/nuget/v/StevanFreeborn.Results.svg)](https://www.nuget.org/packages/StevanFreeborn.Results)
[![NuGet](https://img.shields.io/nuget/dt/StevanFreeborn.Results.svg)](https://www.nuget.org/packages/StevanFreeborn.Results)
[![Build](https://github.com/StevanFreeborn/stevanfreeborn.results/actions/workflows/ci.yaml/badge.svg)](https://github.com/StevanFreeborn/stevanfreeborn.results/actions/workflows/ci.yaml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A minimalistic, AOT-compatible Result type library with support for custom error types, for railway-oriented programming and functional error handling in .NET.

## Features

- **Railway-Oriented Programming**: Chain operations that can fail without nested try-catch blocks
- **Custom Error Types**: Define your own error types that implement `IError`
- **AOT Compatible**: Works with Native AOT and trimming
- **Nullable Reference Types**: Full support for nullable reference types
- **No External Dependencies**: Lightweight with zero dependencies
- **Comprehensive XML Documentation**: Full IntelliSense support

## Installation

Install via NuGet:

```bash
dotnet add package StevanFreeborn.Results
```

## Quick Start

```csharp
using StevanFreeborn.Results;

// Create a successful result
Result<Unit, Error> ok = Result.Ok<Unit, Error>(default);

// Create a failed result
Result<Unit, Error> fail = Result.Fail<Unit, Error>(new Error("NotFound", "User not found"));

// Work with results that have values
Result<int, Error> divisionResult = Divide(10, 2);

if (divisionResult.IsSuccess)
{
  Console.WriteLine(divisionResult.Value); // 5
}

// Railway-oriented programming with Bind
Result<User, Error> GetUser(int id) => ...;
Result<Order, Error> GetOrder(int orderId) => ...;

// Chain operations without exceptions
Result<Order, Error> GetUserOrder(int userId, int orderId)
{
  return GetUser(userId)
    .Bind(user => GetOrder(user.OrderId))
    .Map(order => order.WithUserDetails(user));
}
```

## Custom Error Types

One of the key features is the ability to define your own error types:

```csharp
// Define a custom error
public record DomainError(string Code, string Message) : IError;

// Use with Result
Result<User, DomainError> GetUser(int id)
{
  if (id <= 0)
  {
    return Result.Fail<User, DomainError>(new DomainError("InvalidId", "User ID must be positive"));
  }
  
  // ... fetch user
  return Result.Ok<User, DomainError>(user);
}

// Chain with custom errors
Result<Order, DomainError> result = GetUser(1)
  .Bind(user => GetOrder(user.OrderId))
  .MapError(e => new DomainError(e.Code, $"Failed to get order: {e.Message}"));
```

## IError Interface

The `IError` interface is the foundation for custom error types:

```csharp
public interface IError
{
  string Code { get; }
  string Message { get; }
}
```

Any type implementing `IError` can be used with `Result<T, TError>`.

## Built-in Error Class

The library includes a built-in `Error` class that implements `IError`:

```csharp
// Create an error
var error = new Error("NotFound", "User not found");

// With metadata
var errorWithMetadata = new Error(
  "ValidationFailed", "Invalid input", 
  new Dictionary<string, object> { { "Field", "email" } }
);
```

## Unit Type

The `Unit` struct represents a void-like type for use when no value is needed:

```csharp
// For operations that only succeed or fail without a value
Result<Unit, Error> operation = DoSomething();
```

## Result Types

### `Result` (Static Factory Class)

The static `Result` class provides factory methods for creating result instances:

```csharp
// Create successful result
Result<int, Error> ok = Result.Ok<int, Error>(42);

// Create failed result
Result<int, Error> fail = Result.Fail<int, Error>(new Error("Failed", "Something went wrong"));

// Wrap a function that may throw
Result<string, Error> result = Result.Try<string, Error>(() => File.ReadAllText("file.txt"));
```

### `Result<T, TError>`

The main Result type with generic type parameters for both value and error:

```csharp
// Creation
Result<int, Error> ok = Result.Ok<int, Error>(42);
Result<int, Error> fail = Result.Fail<int, Error>(new Error("Invalid", "Invalid input"));

// Check status
if (result.IsSuccess) { /* ... */ }
if (result.IsFailure) { /* ... */ }

// Access value (throws on failure)
int value = result.Value;

// Access error (throws on success)
Error error = result.Error;
```

### Result without value

For operations that don't return a value, use `Unit`:

```csharp
Result<Unit, Error> ok = Result.Ok<Unit, Error>(default);
Result<Unit, Error> fail = Result.Fail<Unit, Error>(new Error("Failed", "Something went wrong"));
```

## Functional Operations

### Map

Transforms the value if success, propagates the error if failure.

```csharp
Result<int, Error> ok = Result.Ok<int, Error>(5);
Result<string, Error> mapped = ok.Map(x => x.ToString()); // Result.Ok<string, Error>("5")
```

### MapError

Transforms the error if failure, propagates the value if success.

```csharp
Result<int, Error> fail = Result.Fail<int, Error>(new Error("NotFound", "Not found"));
Result<int, Error> mapped = fail.MapError(e => new Error("Unexpected", e.Message));
```

### Bind

Chains another operation that returns a Result.

```csharp
Result<User, Error> GetUser(int id) => ...;
Result<Order, Error> GetOrder(int userId) => ...;

Result<Order, Error> GetUserOrder(int userId)
{
  return GetUser(userId)
    .Bind(user => GetOrder(user.Id));
}
```

### Match

Executes different functions based on success or failure.

```csharp
Result<int, Error> result = Divide(10, 2);

// Get a value
string message = result.Match(
  onSuccess: value => $"Result: {value}",
  onFailure: error => $"Error: {error.Message}"
);

// Execute actions
result.Match(
  onSuccess: value => Console.WriteLine(value),
  onFailure: error => Console.WriteLine(error.Message)
);
```

### Try

Wraps a function that may throw an exception in a Result.

```csharp
// Simple usage with default error handler
Result<string, Error> result = Result.Try<string, Error>(() => File.ReadAllText("file.txt"));

// Custom error handler
Result<string, Error> result = Result.Try<string, Error>(
  () => File.ReadAllText("file.txt"),
  ex => new Error("ReadError", ex.Message)
);
```

## Async Operations

The library provides async extension methods for all functional operations.

```csharp
// Async Map
Result<User, Error> user = await GetUserAsync(id);
Result<string, Error> userName = await user.MapAsync(u => GetNameAsync(u));

// Async Bind
Result<Order, Error> order = await GetUserAsync(id)
  .BindAsync(user => GetOrderAsync(user.OrderId));

// Async Match
string result = await result.MatchAsync(
  onSuccess: async value => await ProcessAsync(value),
  onFailure: async error => await HandleErrorAsync(error)
);

// Async Try
Result<string, Error> result = await Result.TryAsync<string, Error>(
  () => HttpClient.GetStringAsync("https://api.example.com")
);
```

## Extension Methods

### Result<T, TError> Methods

| Method                                           | Description                        |
|--------------------------------------------------|------------------------------------|
| `Map(Func<T, TNew>)`                             | Transforms the value               |
| `Map(Func<T, TNew>, Func<TError, TNewError>)`    | Transforms the value or error      |
| `MapError(Func<TError, TNewError>)`              | Transforms the error               |
| `Bind(Func<T, Result<TNew, TError>>)`            | Chains a new Result                |
| `Match(Func<T, TResult>, Func<TError, TResult>)` | Pattern matching returning a value |
| `Match(Action<T>, Action<TError>)`               | Pattern matching executing actions |
| `Try(Func<T>)`                                   | Wraps a function that may throw    |

### Async Extensions

| Method          | Description               |
|-----------------|---------------------------|
| `MapAsync`      | Async version of Map      |
| `MapErrorAsync` | Async version of MapError |
| `BindAsync`     | Async version of Bind     |
| `MatchAsync`    | Async version of Match    |
| `TryAsync`      | Async version of Try      |

## Requirements

- .NET Standard 2.1
- .NET 10.0+

## License

MIT License - see [LICENSE.md](LICENSE.md) for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
