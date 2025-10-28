namespace Server.SharedKernel.Result;

public interface IResult
{
  ResultStatus Status { get; }
  IEnumerable<ValidationError> ValidationErrors { get; }
  Type ValueType { get; }
  object? GetValue();
  string Location { get; }
}
