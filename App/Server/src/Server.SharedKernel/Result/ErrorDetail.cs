namespace Server.SharedKernel.Result;

public class ErrorDetail
{
  public ErrorDetail()
  {
  }

  public ErrorDetail(string errorMessage) => ErrorMessage = errorMessage;

  public ErrorDetail(string identifier, string errorMessage)
  {
    Identifier = identifier;
    ErrorMessage = errorMessage;
  }

  public ErrorDetail(string identifier, string errorMessage, string errorCode, ValidationSeverity severity)
  {
    Identifier = identifier;
    ErrorMessage = errorMessage;
    ErrorCode = errorCode;
    Severity = severity;
  }

  public string Identifier { get; set; } = string.Empty;

  public string ErrorMessage { get; set; } = string.Empty;

  public string ErrorCode { get; set; } = string.Empty;

  public ValidationSeverity Severity { get; set; } = ValidationSeverity.Error;
}
