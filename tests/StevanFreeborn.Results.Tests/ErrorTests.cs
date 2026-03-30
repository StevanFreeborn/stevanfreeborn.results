namespace StevanFreeborn.Results.Tests;

public class ErrorTests
{
  [Test]
  public async Task Constructor_WhenCalled_ItShouldSetCode()
  {
    var error = new Error("code", "message");

    await Assert.That(error.Code).IsEqualTo("code");
  }

  [Test]
  public async Task Constructor_WhenCalled_ItShouldSetMessage()
  {
    var error = new Error("code", "message");

    await Assert.That(error.Message).IsEqualTo("message");
  }

  [Test]
  public async Task Constructor_WhenCalledWithMetadata_ItShouldSetMetadata()
  {
    var metadata = new Dictionary<string, object> { { "key", "value" } };
    var error = new Error("code", "message", metadata);

    await Assert.That(error.Metadata!.Count).IsEqualTo(1);
    await Assert.That(error.Metadata["key"]).IsEqualTo("value");
  }

  [Test]
  public async Task Constructor_WhenCalledWithNullMetadata_ItShouldSetMetadataToNull()
  {
    var error = new Error("code", "message", null);

    await Assert.That(error.Metadata).IsNull();
  }

  [Test]
  public async Task Equals_WhenErrorsHaveSameCodeAndMessage_ItShouldReturnTrue()
  {
    var error1 = new Error("code", "message");
    var error2 = new Error("code", "message");

    await Assert.That(error1).IsEqualTo(error2);
  }

  [Test]
  public async Task Equals_WhenErrorsHaveDifferentCode_ItShouldReturnFalse()
  {
    var error1 = new Error("code1", "message");
    var error2 = new Error("code2", "message");

    await Assert.That(error1 == error2).IsFalse();
  }

  [Test]
  public async Task Error_WhenCreated_ItShouldImplementIError()
  {
    var error = new Error("code", "message");

    await Assert.That(error).IsAssignableTo<IError>();
  }
}
