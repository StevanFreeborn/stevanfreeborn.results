using System.Globalization;

namespace StevanFreeborn.Results.Tests;

public class ResultTTests
{
  [Test]
  public async Task Ok_WhenCalledWithValue_ItShouldReturnSuccessResult()
  {
    var result = Result<string, Error>.Ok("test");
    await Assert.That(result.IsSuccess).IsTrue();
    await Assert.That(result.IsFailure).IsFalse();
    await Assert.That(result.Value).IsEqualTo("test");
  }

  [Test]
  public async Task Fail_WhenCalled_ItShouldReturnFailureResult()
  {
    var error = new Error("code", "message");
    var result = Result<string, Error>.Fail(error);
    await Assert.That(result.IsFailure).IsTrue();
    await Assert.That(result.IsSuccess).IsFalse();
    await Assert.That(result.Error).IsEqualTo(error);
  }

  [Test]
  public async Task Value_WhenResultIsFailure_ItShouldThrowInvalidOperationException()
  {
    var result = Result<string, Error>.Fail(new Error("code", "message"));

    Assert.Throws<InvalidOperationException>(() => _ = result.Value);
  }

  [Test]
  public async Task Error_WhenResultIsSuccess_ItShouldThrowInvalidOperationException()
  {
    var result = Result<string, Error>.Ok("test");

    Assert.Throws<InvalidOperationException>(() => _ = result.Error);
  }

  [Test]
  public async Task ImplicitConversion_FromValue_ItShouldReturnSuccessResult()
  {
    Result<string, Error> result = "test";
    await Assert.That(result.IsSuccess).IsTrue();
    await Assert.That(result.Value).IsEqualTo("test");
  }

  [Test]
  public async Task ImplicitConversion_FromError_ItShouldReturnFailureResult()
  {
    var error = new Error("code", "message");
    Result<string, Error> result = error;
    await Assert.That(result.IsFailure).IsTrue();
    await Assert.That(result.Error).IsEqualTo(error);
  }

  [Test]
  public async Task Map_WhenResultIsSuccess_ItShouldTransformValue()
  {
    var result = Result<int, Error>.Ok(5);
    var mapped = result.Map(n => n.ToString(CultureInfo.InvariantCulture));
    await Assert.That(mapped.IsSuccess).IsTrue();
    await Assert.That(mapped.Value).IsEqualTo("5");
  }

  [Test]
  public async Task Map_WhenResultIsFailure_ItShouldPropagateError()
  {
    var error = new Error("code", "message");
    var result = Result<int, Error>.Fail(error);
    var mapped = result.Map(n => n.ToString(CultureInfo.InvariantCulture));
    await Assert.That(mapped.IsFailure).IsTrue();
    await Assert.That(mapped.Error).IsEqualTo(error);
  }

  [Test]
  public async Task MapError_WhenResultIsFailure_ItShouldTransformError()
  {
    var originalError = new Error("original", "original message");
    var result = Result<int, Error>.Fail(originalError);
    var transformed = result.MapError(e => new Error("transformed", e.Message));
    await Assert.That(transformed.IsFailure).IsTrue();
    await Assert.That(transformed.Error.Code).IsEqualTo("transformed");
  }

  [Test]
  public async Task MapError_WhenResultIsSuccess_ItShouldReturnUnchanged()
  {
    var result = Result<int, Error>.Ok(5);
    var mapped = result.MapError(e => new Error("transformed", e.Message));
    await Assert.That(mapped.IsSuccess).IsTrue();
    await Assert.That(mapped.Value).IsEqualTo(5);
  }

  [Test]
  public async Task Bind_WhenResultIsSuccess_ItShouldChainToBinderResult()
  {
    var result = Result<int, Error>.Ok(5);
    var bound = result.Bind(n => Result<string, Error>.Ok(n.ToString(CultureInfo.InvariantCulture)));
    await Assert.That(bound.IsSuccess).IsTrue();
    await Assert.That(bound.Value).IsEqualTo("5");
  }

  [Test]
  public async Task Bind_WhenResultIsFailure_ItShouldPropagateError()
  {
    var error = new Error("code", "message");
    var result = Result<int, Error>.Fail(error);
    var bound = result.Bind(n => Result<string, Error>.Ok(n.ToString(CultureInfo.InvariantCulture)));
    await Assert.That(bound.IsFailure).IsTrue();
    await Assert.That(bound.Error).IsEqualTo(error);
  }

  [Test]
  public async Task Bind_WhenResultIsSuccess_ItShouldReturnBinderFailureResult()
  {
    var result = Result<int, Error>.Ok(5);
    var bound = result.Bind(_ => Result<string, Error>.Fail(new Error("bind_error", "bound failed")));
    await Assert.That(bound.IsFailure).IsTrue();
    await Assert.That(bound.Error.Code).IsEqualTo("bind_error");
  }

  [Test]
  public async Task Match_WhenResultIsSuccess_ItShouldReturnOnSuccessValue()
  {
    var result = Result<string, Error>.Ok("test");
    var matched = result.Match(v => v.Length, _ => 0);
    await Assert.That(matched).IsEqualTo(4);
  }

  [Test]
  public async Task Match_WhenResultIsFailure_ItShouldReturnOnFailureValue()
  {
    var result = Result<string, Error>.Fail(new Error("code", "message"));
    var matched = result.Match(v => v.Length, e => -1);
    await Assert.That(matched).IsEqualTo(-1);
  }

  [Test]
  public async Task Match_WhenResultIsSuccess_ItShouldInvokeOnSuccess()
  {
    var result = Result<string, Error>.Ok("test");
    var invoked = false;
    result.Match(v => { invoked = true; }, _ => { });
    await Assert.That(invoked).IsTrue();
  }

  [Test]
  public async Task Match_WhenResultIsFailure_ItShouldInvokeOnFailure()
  {
    var error = new Error("code", "message");
    var result = Result<string, Error>.Fail(error);
    var invoked = false;
    result.Match(_ => { }, e => { invoked = true; });
    await Assert.That(invoked).IsTrue();
  }

  [Test]
  public async Task Try_WhenFuncSucceeds_ItShouldReturnOkWithValue()
  {
    var result = Result<int, Error>.Try(() => 42);
    await Assert.That(result.IsSuccess).IsTrue();
    await Assert.That(result.Value).IsEqualTo(42);
  }

  [Test]
  public async Task Try_WhenFuncThrowsException_ItShouldReturnFailWithUnexpectedError()
  {
    var result = Result<int, Error>.Try(() => throw new InvalidOperationException("test error"));
    await Assert.That(result.IsFailure).IsTrue();
    await Assert.That(result.Error.Code).IsEqualTo("UnexpectedError");
  }

  [Test]
  public async Task Try_WhenFuncThrowsException_ItShouldUseCustomError()
  {
    var result = Result<int, Error>.Try(() => throw new InvalidOperationException("test"), ex => new Error("custom", ex.Message));
    await Assert.That(result.IsFailure).IsTrue();
    await Assert.That(result.Error.Code).IsEqualTo("custom");
  }
}
