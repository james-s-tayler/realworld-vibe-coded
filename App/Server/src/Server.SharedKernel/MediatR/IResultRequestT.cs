using MediatR;
using Server.SharedKernel.Result;

namespace Server.SharedKernel.MediatR;

/// <summary>
/// Generic marker interface for requests that return Result{T}.
/// This interface exposes the inner type T at compile time, allowing pipeline behaviors
/// to work with the inner type without using reflection on Result{T} itself.
/// </summary>
/// <typeparam name="T">The inner value type of Result{T}</typeparam>
public interface IResultRequest<T> : IResultRequest, IRequest<Result<T>>
{
}
