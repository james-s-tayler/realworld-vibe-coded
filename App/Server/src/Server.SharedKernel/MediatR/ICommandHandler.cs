using MediatR;
using Server.SharedKernel.Result;

namespace Server.SharedKernel.MediatR;

/// <summary>
/// Handler for commands that return Result{T}.
/// Source: https://code-maze.com/cqrs-mediatr-fluentvalidation/
/// </summary>
/// <typeparam name="TCommand">The command type</typeparam>
/// <typeparam name="T">The inner value type of Result{T}</typeparam>
public interface ICommandHandler<in TCommand, T> : IRequestHandler<TCommand, Result<T>>
        where TCommand : ICommand<T>
{
}
