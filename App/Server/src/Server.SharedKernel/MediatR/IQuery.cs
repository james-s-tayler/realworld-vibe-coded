namespace Server.SharedKernel.MediatR;

/// <summary>
/// Represents a query that returns a Result{T}.
/// Implements IResultRequest{T} to expose the inner type T at compile time,
/// enabling pipeline behaviors to work with the type directly without using reflection.
/// Source: https://code-maze.com/cqrs-mediatr-fluentvalidation/
/// </summary>
/// <typeparam name="T">The inner value type of Result{T}</typeparam>
public interface IQuery<T> : IResultRequest<T>
{
}
