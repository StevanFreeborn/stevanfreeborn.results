using System.Globalization;

namespace StevanFreeborn.Results.Tests;

public class ResultAsyncExtensionsTests
{
  [Test]
  public async Task MapAsync_WhenResultIsSuccess_ItShouldInvokeAsyncAction()
  {
    var result = Result.Ok<Unit, Error>(default);
    var invoked = false;

    await result.MapAsync(_ => { invoked = true; return Task.CompletedTask; });

    await Assert.That(invoked).IsTrue();
  }

  [Test]
  public async Task MapAsync_WhenResultIsFailure_ItShouldSkipAsyncAction()
  {
    var result = Result.Fail<Unit, Error>(new Error("code", "message"));
    var invoked = false;

    await result.MapAsync(_ => { invoked = true; return Task.CompletedTask; });

    await Assert.That(invoked).IsFalse();
  }

  [Test]
  public async Task MapAsync_WhenCalled_ItShouldReturnOriginalResult()
  {
    var result = Result.Ok<Unit, Error>(default);
    var mapped = await result.MapAsync(_ => Task.CompletedTask);

    await Assert.That(mapped.IsSuccess).IsTrue();
  }

  [Test]
  public async Task BindAsync_WhenResultIsSuccess_ItShouldInvokeBinderAndReturnResult()
  {
    var result = Result.Ok<Unit, Error>(default);
    var bound = await result.BindAsync(_ => Task.FromResult(Result.Fail<Unit, Error>(new Error("new_error", "new message"))));

    await Assert.That(bound.IsFailure).IsTrue();
  }

  [Test]
  public async Task BindAsync_WhenResultIsFailure_ItShouldReturnOriginalResult()
  {
    var result = Result.Fail<Unit, Error>(new Error("code", "message"));
    var bound = await result.BindAsync(_ => Task.FromResult(Result.Ok<Unit, Error>(default)));

    await Assert.That(bound.IsFailure).IsTrue();
    await Assert.That(bound.Error.Code).IsEqualTo("code");
  }

  [Test]
  public async Task MatchAsync_WhenResultIsSuccess_ItShouldReturnOnSuccessValue()
  {
    var result = Result.Ok<Unit, Error>(default);
    var matched = await result.MatchAsync(_ => Task.FromResult("success"), _ => Task.FromResult("failure"));

    await Assert.That(matched).IsEqualTo("success");
  }

  [Test]
  public async Task MatchAsync_WhenResultIsFailure_ItShouldReturnOnFailureValue()
  {
    var result = Result.Fail<Unit, Error>(new Error("code", "message"));
    var matched = await result.MatchAsync(_ => Task.FromResult("success"), e => Task.FromResult(e.Code));

    await Assert.That(matched).IsEqualTo("code");
  }

  [Test]
  public async Task TryAsync_WhenActionSucceeds_ItShouldReturnOk()
  {
    var result = await ResultAsyncExtensions.TryAsync(() => Task.CompletedTask);

    await Assert.That(result.IsSuccess).IsTrue();
  }

  [Test]
  public async Task TryAsync_WhenActionThrowsException_ItShouldReturnFailWithUnexpectedError()
  {
    var result = await ResultAsyncExtensions.TryAsync(() => throw new InvalidOperationException("test error"));

    await Assert.That(result.IsFailure).IsTrue();
    await Assert.That(result.Error.Code).IsEqualTo("UnexpectedError");
  }

  [Test]
  public async Task TryAsync_WhenActionThrowsException_ItShouldUseCustomError()
  {
    var result = await ResultAsyncExtensions.TryAsync(
      () => throw new InvalidOperationException("test"),
      ex => new Error("custom", ex.Message)
    );

    await Assert.That(result.IsFailure).IsTrue();
    await Assert.That(result.Error.Code).IsEqualTo("custom");
  }

  [Test]
  public async Task MapAsync_WhenResultIsSuccess_ItShouldTransformValue()
  {
    var result = Result.Ok<int, Error>(5);
    var mapped = await result.MapAsync(async n => (await Task.FromResult(n)).ToString(CultureInfo.InvariantCulture));

    await Assert.That(mapped.IsSuccess).IsTrue();
    await Assert.That(mapped.Value).IsEqualTo("5");
  }

  [Test]
  public async Task MapAsync_WhenResultIsFailure_ItShouldPropagateError()
  {
    var error = new Error("code", "message");
    var result = Result.Fail<int, Error>(error);
    var mapped = await result.MapAsync(async n => (await Task.FromResult(n)).ToString(CultureInfo.InvariantCulture));

    await Assert.That(mapped.IsFailure).IsTrue();
    await Assert.That(mapped.Error).IsEqualTo(error);
  }

  [Test]
  public async Task BindAsync_WhenResultIsSuccess_ItShouldChainToBinderResult()
  {
    var result = Result.Ok<int, Error>(5);
    var bound = await result.BindAsync(async n => await Task.FromResult(Result.Ok<string, Error>(n.ToString(CultureInfo.InvariantCulture))));

    await Assert.That(bound.IsSuccess).IsTrue();
    await Assert.That(bound.Value).IsEqualTo("5");
  }

  [Test]
  public async Task BindAsync_WhenResultIsFailure_ItShouldPropagateError()
  {
    var error = new Error("code", "message");
    var result = Result.Fail<int, Error>(error);
    var bound = await result.BindAsync(async n => await Task.FromResult(Result.Ok<string, Error>(n.ToString(CultureInfo.InvariantCulture))));

    await Assert.That(bound.IsFailure).IsTrue();
    await Assert.That(bound.Error).IsEqualTo(error);
  }

  [Test]
  public async Task MatchAsyncGeneric_WhenResultIsSuccess_ItShouldReturnOnSuccessValue()
  {
    var result = Result.Ok<string, Error>("test");
    var matched = await result.MatchAsync(
      v => Task.FromResult(v.Length),
      _ => Task.FromResult(0)
    );

    await Assert.That(matched).IsEqualTo(4);
  }

  [Test]
  public async Task MatchAsyncGeneric_WhenResultIsFailure_ItShouldReturnOnFailureValue()
  {
    var result = Result.Fail<string, Error>(new Error("code", "message"));
    var matched = await result.MatchAsync(
      v => Task.FromResult(v.Length),
      _ => Task.FromResult(-1)
    );

    await Assert.That(matched).IsEqualTo(-1);
  }

  [Test]
  public async Task TryAsync_WhenFuncSucceeds_ItShouldReturnOkWithValue()
  {
    var result = await ResultAsyncExtensions.TryAsync(async () => await Task.FromResult(42));

    await Assert.That(result.IsSuccess).IsTrue();
    await Assert.That(result.Value).IsEqualTo(42);
  }

  [Test]
  public async Task TryAsync_WhenFuncThrowsException_ItShouldReturnFailWithUnexpectedError()
  {
    var result = await ResultAsyncExtensions.TryAsync<int>(() => throw new InvalidOperationException("test error"));

    await Assert.That(result.IsFailure).IsTrue();
    await Assert.That(result.Error.Code).IsEqualTo("UnexpectedError");
  }

  [Test]
  public async Task TryAsync_WhenFuncThrowsException_ItShouldUseCustomError()
  {
    var result = await ResultAsyncExtensions.TryAsync<int>(
      () => throw new InvalidOperationException("test"),
      ex => new Error("custom", ex.Message)
    );

    await Assert.That(result.IsFailure).IsTrue();
    await Assert.That(result.Error.Code).IsEqualTo("custom");
  }
}
