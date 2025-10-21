namespace Server.SharedKernel;

public class ValidationError
{
  public ValidationError()
  {
  }

  public ValidationError(string errorMessage) => ErrorMessage = errorMessage;

  public ValidationError(string identifier, string errorMessage)
  {
    Identifier = identifier;
    ErrorMessage = errorMessage;
  }

  public ValidationError(string identifier, string errorMessage, string errorCode, ValidationSeverity severity)
  {
    Identifier = identifier;
    ErrorMessage = errorMessage;
    ErrorCode = errorCode;
    Severity = severity;
  }

  public string? Identifier { get; set; }
  public string? ErrorMessage { get; set; }
  public string? ErrorCode { get; set; }
  public ValidationSeverity Severity { get; set; } = ValidationSeverity.Error;
}
