namespace StevanFreeborn.Results.Tests;

public class ResultTests
{
  [Test]
  public async Task Ok_WhenCalled_ItShouldReturnSuccessResult()
  {
    var result = Result.Ok<Unit, Error>(default);

    await Assert.That(result.IsSuccess).IsTrue();
    await Assert.That(result.IsFailure).IsFalse();
  }

  [Test]
  public async Task Fail_WhenCalled_ItShouldReturnFailureResult()
  {
    var error = new Error("code", "message");
    var result = Result.Fail<Unit, Error>(error);

    await Assert.That(result.IsSuccess).IsFalse();
    await Assert.That(result.IsFailure).IsTrue();
    await Assert.That(result.Error).IsEqualTo(error);
  }

  [Test]
  public void Error_WhenResultIsSuccess_ItShouldThrowInvalidOperationException()
  {
    Assert.Throws<InvalidOperationException>(() => _ = Result.Ok<Unit, Error>(default).Error);
  }

  [Test]
  public async Task ImplicitConversion_FromError_ItShouldReturnFailureResult()
  {
    var error = new Error("code", "message");
    Result<Unit, Error> result = error;

    await Assert.That(result.IsFailure).IsTrue();
    await Assert.That(result.Error).IsEqualTo(error);
  }

  [Test]
  public async Task Map_WhenResultIsSuccess_ItShouldInvokeAction()
  {
    var result = Result.Ok<Unit, Error>(default);
    var invoked = false;

    result.Map(_ => invoked = true);

    await Assert.That(invoked).IsTrue();
  }

  [Test]
  public async Task Map_WhenResultIsFailure_ItShouldSkipAction()
  {
    var result = Result.Fail<Unit, Error>(new Error("code", "message"));
    var invoked = false;

    result.Map(_ => invoked = true);

    await Assert.That(invoked).IsFalse();
  }

  [Test]
  public async Task Map_WhenCalled_ItShouldReturnOriginalResult()
  {
    var result = Result.Ok<Unit, Error>(default);
    var mapped = result.Map(_ => { });

    await Assert.That(mapped.IsSuccess).IsTrue();
  }

  [Test]
  public async Task Bind_WhenResultIsSuccess_ItShouldInvokeBinder()
  {
    var result = Result.Ok<Unit, Error>(default);
    var bound = result.Bind(_ => Result.Ok<Unit, Error>(default));

    await Assert.That(bound.IsSuccess).IsTrue();
  }

  [Test]
  public async Task Bind_WhenResultIsFailure_ItShouldReturnOriginalResult()
  {
    var result = Result.Fail<Unit, Error>(new Error("code", "message"));
    var bound = result.Bind(_ => Result.Ok<Unit, Error>(default));

    await Assert.That(bound.IsFailure).IsTrue();
    await Assert.That(bound.Error).IsEqualTo(result.Error);
  }

  [Test]
  public async Task Bind_WhenResultIsSuccess_ItShouldReturnBinderResult()
  {
    var result = Result.Ok<Unit, Error>(default);
    var bound = result.Bind(_ => Result.Fail<Unit, Error>(new Error("new_error", "new message")));

    await Assert.That(bound.IsFailure).IsTrue();
  }

  [Test]
  public async Task Match_WhenResultIsSuccess_ItShouldReturnOnSuccessValue()
  {
    var result = Result.Ok<Unit, Error>(default);
    var matched = result.Match(_ => "success", _ => "failure");

    await Assert.That(matched).IsEqualTo("success");
  }

  [Test]
  public async Task Match_WhenResultIsFailure_ItShouldReturnOnFailureValue()
  {
    var result = Result.Fail<Unit, Error>(new Error("code", "message"));
    var matched = result.Match(_ => "success", e => e.Code);

    await Assert.That(matched).IsEqualTo("code");
  }

  [Test]
  public async Task Match_WhenResultIsSuccess_ItShouldInvokeOnSuccess()
  {
    var result = Result.Ok<Unit, Error>(default);
    var invoked = false;

    result.Match(_ => invoked = true, _ => { });

    await Assert.That(invoked).IsTrue();
  }

  [Test]
  public async Task Match_WhenResultIsFailure_ItShouldInvokeOnFailure()
  {
    var error = new Error("code", "message");
    var result = Result.Fail<Unit, Error>(error);
    var invoked = false;

    result.Match(_ => { }, e => invoked = true);

    await Assert.That(invoked).IsTrue();
  }

  [Test]
  public async Task Try_WhenActionSucceeds_ItShouldReturnOk()
  {
    var result = Result.Try<Unit, Error>(() => default);

    await Assert.That(result.IsSuccess).IsTrue();
  }

  [Test]
  public async Task Try_WhenActionThrowsException_ItShouldReturnFailWithUnexpectedError()
  {
    var result = Result.Try<Unit, Error>(() => throw new Exception("test error"));

    await Assert.That(result.IsFailure).IsTrue();
    await Assert.That(result.Error.Code).IsEqualTo("UnexpectedError");
  }

  [Test]
  public async Task Try_WhenActionThrowsException_ItShouldUseCustomError()
  {
    var result = Result.Try<Unit, Error>(() => throw new Exception("test"), ex => new Error("custom", ex.Message));

    await Assert.That(result.IsFailure).IsTrue();
    await Assert.That(result.Error.Code).IsEqualTo("custom");
  }
}