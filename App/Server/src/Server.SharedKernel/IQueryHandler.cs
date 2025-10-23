using Ardalis.Result;
using MediatR;

namespace Server.SharedKernel;

/// <summary>
/// Handler for queries that return Result{T}.
/// Source: https://code-maze.com/cqrs-mediatr-fluentvalidation/
/// </summary>
/// <typeparam name="TQuery">The query type</typeparam>
/// <typeparam name="T">The inner value type of Result{T}</typeparam>
public interface IQueryHandler<in TQuery, T> : IRequestHandler<TQuery, Result<T>>
       where TQuery : IQuery<T>
{
}
