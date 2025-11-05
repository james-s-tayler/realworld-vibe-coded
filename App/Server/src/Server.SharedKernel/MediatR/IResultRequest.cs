namespace Server.SharedKernel.MediatR;

/// <summary>
/// Non-generic marker interface for requests that return Result{T}.
/// Used for runtime type checking in pipeline behaviors.
/// </summary>
public interface IResultRequest
{
}
