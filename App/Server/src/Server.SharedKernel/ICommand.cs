namespace Server.SharedKernel;

/// <summary>
/// Represents a command that returns a Result{T}.
/// Implements IResultRequest{T} to expose the inner type T at compile time,
/// enabling pipeline behaviors to work with the type directly without using reflection.
/// Source: https://code-maze.com/cqrs-mediatr-fluentvalidation/
/// </summary>
/// <typeparam name="T">The inner value type of Result{T}</typeparam>
public interface ICommand<T> : IResultRequest<T>
{
}
