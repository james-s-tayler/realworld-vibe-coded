using Ardalis.Result;
using MediatR;

namespace Server.SharedKernel;

/// <summary>
/// Handler for commands.
/// Commands must return either Result or Result&lt;T&gt; to ensure consistent error handling.
/// Source: https://code-maze.com/cqrs-mediatr-fluentvalidation/
/// </summary>
/// <typeparam name="TCommand"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
        where TResponse : IResult
{
}
