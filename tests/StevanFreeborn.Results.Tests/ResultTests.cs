namespace StevanFreeborn.Results.Tests;

public class ResultTests
{
  [Test]
  public async Task Ok_WhenCalled_ItShouldReturnSuccessResult()
  {
    var result = Result<Unit, Error>.Ok(default);
    await Assert.That(result.IsSuccess).IsTrue();
    await Assert.That(result.IsFailure).IsFalse();
  }

  [Test]
  public async Task Fail_WhenCalled_ItShouldReturnFailureResult()
  {
    var error = new Error("code", "message");
    var result = Result<Unit, Error>.Fail(error);
    await Assert.That(result.IsSuccess).IsFalse();
    await Assert.That(result.IsFailure).IsTrue();
    await Assert.That(result.Error).IsEqualTo(error);
  }

  [Test]
  public async Task Error_WhenResultIsSuccess_ItShouldThrowInvalidOperationException()
  {
    Assert.Throws<InvalidOperationException>(() => _ = Result<Unit, Error>.Ok(default).Error);
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
    var result = Result<Unit, Error>.Ok(default);
    var invoked = false;
    result.Map(_ => invoked = true);
    await Assert.That(invoked).IsTrue();
  }

  [Test]
  public async Task Map_WhenResultIsFailure_ItShouldSkipAction()
  {
    var result = Result<Unit, Error>.Fail(new Error("code", "message"));
    var invoked = false;
    result.Map(_ => invoked = true);
    await Assert.That(invoked).IsFalse();
  }

  [Test]
  public async Task Map_WhenCalled_ItShouldReturnOriginalResult()
  {
    var result = Result<Unit, Error>.Ok(default);
    var mapped = result.Map(_ => { });
    await Assert.That(mapped.IsSuccess).IsTrue();
  }

  [Test]
  public async Task Bind_WhenResultIsSuccess_ItShouldInvokeBinder()
  {
    var result = Result<Unit, Error>.Ok(default);
    var bound = result.Bind(_ => Result<Unit, Error>.Ok(default));
    await Assert.That(bound.IsSuccess).IsTrue();
  }

  [Test]
  public async Task Bind_WhenResultIsFailure_ItShouldReturnOriginalResult()
  {
    var result = Result<Unit, Error>.Fail(new Error("code", "message"));
    var bound = result.Bind(_ => Result<Unit, Error>.Ok(default));
    await Assert.That(bound.IsFailure).IsTrue();
    await Assert.That(bound.Error).IsEqualTo(result.Error);
  }

  [Test]
  public async Task Bind_WhenResultIsSuccess_ItShouldReturnBinderResult()
  {
    var result = Result<Unit, Error>.Ok(default);
    var bound = result.Bind(_ => Result<Unit, Error>.Fail(new Error("new_error", "new message")));
    await Assert.That(bound.IsFailure).IsTrue();
  }

  [Test]
  public async Task Match_WhenResultIsSuccess_ItShouldReturnOnSuccessValue()
  {
    var result = Result<Unit, Error>.Ok(default);
    var matched = result.Match(_ => "success", _ => "failure");
    await Assert.That(matched).IsEqualTo("success");
  }

  [Test]
  public async Task Match_WhenResultIsFailure_ItShouldReturnOnFailureValue()
  {
    var result = Result<Unit, Error>.Fail(new Error("code", "message"));
    var matched = result.Match(_ => "success", e => e.Code);
    await Assert.That(matched).IsEqualTo("code");
  }

  [Test]
  public async Task Match_WhenResultIsSuccess_ItShouldInvokeOnSuccess()
  {
    var result = Result<Unit, Error>.Ok(default);
    var invoked = false;
    result.Match(_ => invoked = true, _ => { });
    await Assert.That(invoked).IsTrue();
  }

  [Test]
  public async Task Match_WhenResultIsFailure_ItShouldInvokeOnFailure()
  {
    var error = new Error("code", "message");
    var result = Result<Unit, Error>.Fail(error);
    var invoked = false;
    result.Match(_ => { }, e => invoked = true);
    await Assert.That(invoked).IsTrue();
  }

  [Test]
  public async Task Try_WhenActionSucceeds_ItShouldReturnOk()
  {
    var result = Result<Unit, Error>.Try(() => default(Unit));
    await Assert.That(result.IsSuccess).IsTrue();
  }

  [Test]
  public async Task Try_WhenActionThrowsException_ItShouldReturnFailWithUnexpectedError()
  {
    var result = Result<Unit, Error>.Try(() => throw new Exception("test error"));
    await Assert.That(result.IsFailure).IsTrue();
    await Assert.That(result.Error.Code).IsEqualTo("UnexpectedError");
  }

  [Test]
  public async Task Try_WhenActionThrowsException_ItShouldUseCustomError()
  {
    var result = Result<Unit, Error>.Try(() => throw new Exception("test"), ex => new Error("custom", ex.Message));
    await Assert.That(result.IsFailure).IsTrue();
    await Assert.That(result.Error.Code).IsEqualTo("custom");
  }
}
