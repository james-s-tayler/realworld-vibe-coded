namespace Server.SharedKernel.Result;

public interface IResult
{
  ResultStatus Status { get; }
  IEnumerable<ErrorDetail> ErrorDetails { get; }
  Type ValueType { get; }
  object? GetValue();
  string Location { get; }
}
