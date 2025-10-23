using Ardalis.Result;
using MediatR;

namespace Server.SharedKernel;

/// <summary>
/// Marker interface for all commands.
/// Commands must return either Result or Result&lt;T&gt; to ensure consistent error handling.
/// Source: https://code-maze.com/cqrs-mediatr-fluentvalidation/
/// </summary>
/// <typeparam name="TResponse">Must be either Result or Result&lt;T&gt;</typeparam>
public interface ICommand<out TResponse> : IRequest<TResponse>
  where TResponse : IResult
{
}
