namespace StevanFreeborn.Results
{

  /// <summary>
  /// Represents an error with a code, message, and optional metadata.
  /// </summary>
#pragma warning disable CA1716 // Identifiers should not match keywords
  public sealed record Error : IError
#pragma warning restore CA1716 // Identifiers should not match keywords
  {
    /// <summary>
    /// Gets the error code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the error message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets optional metadata associated with the error.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Metadata { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Error"/> class with the specified code, message, and optional metadata.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="metadata">Optional metadata associated with the error.</param>
    public Error(string code, string message, IReadOnlyDictionary<string, object>? metadata = null)
    {
      Code = code;
      Message = message;
      Metadata = metadata;
    }
  }

}