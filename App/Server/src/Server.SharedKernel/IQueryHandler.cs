using Ardalis.Result;
using MediatR;

namespace Server.SharedKernel;

/// <summary>
/// Handler for queries.
/// Queries must return either Result or Result&lt;T&gt; to ensure consistent error handling.
/// Source: https://code-maze.com/cqrs-mediatr-fluentvalidation/
/// </summary>
/// <typeparam name="TQuery"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
       where TQuery : IQuery<TResponse>
       where TResponse : IResult
{
}
