namespace Server.SharedKernel;

/// <summary>
/// Represents a command that returns a Result{T}.
/// Implements IResultRequest{T} to expose the inner type T at compile time,
/// enabling pipeline behaviors to work with the type directly without using reflection.
/// NOTE: MediatR will happily register pipeline behaviors with nested generics in the TResponse,
/// but it won't actually run them at runtime if they are registered via open generics.
/// Thus, getting access to the inner generic type in Result{T} requires that we explicitly
/// register every pipeline behavior, which is a nasty but necessary trade-off.
/// Source: https://code-maze.com/cqrs-mediatr-fluentvalidation/
/// </summary>
/// <typeparam name="T">The inner value type of Result{T}</typeparam>
public interface ICommand<T> : IResultRequest<T>
{
}
