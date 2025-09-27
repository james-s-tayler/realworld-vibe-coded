using System.Net;
using Ardalis.Result;
using FluentValidation;
using MediatR;

namespace Server.Web.Infrastructure;

/// <summary>
/// Pipeline behavior that handles exceptions and converts them to appropriate Result responses
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class ErrorHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IRequest<TResponse>
{
  public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
  {
    try
    {
      return await next();
    }
    catch (ValidationException validationException)
    {
      // Convert FluentValidation exception to Result format if TResponse is Result<T>
      if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
      {
        var validationErrors = validationException.Errors.Select(f => new ValidationError
        {
          Identifier = f.PropertyName,
          ErrorMessage = f.ErrorMessage
        }).ToList();

        var resultType = typeof(TResponse);
        var createMethod = typeof(Result<>).MakeGenericType(resultType.GetGenericArguments())
          .GetMethod("Invalid", new[] { typeof(List<ValidationError>) });

        return (TResponse)createMethod?.Invoke(null, new object[] { validationErrors })!;
      }

      // Re-throw if we can't handle it
      throw;
    }
    catch (UnauthorizedAccessException)
    {
      // Convert unauthorized access to Result format if TResponse is Result<T>
      if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
      {
        var resultType = typeof(TResponse);
        var createMethod = typeof(Result<>).MakeGenericType(resultType.GetGenericArguments())
          .GetMethod("Unauthorized");

        return (TResponse)createMethod?.Invoke(null, null)!;
      }

      throw;
    }
    catch (ArgumentException argumentException)
    {
      // Convert argument exceptions to validation errors if TResponse is Result<T>
      if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
      {
        var errors = new[] { argumentException.Message };
        var resultType = typeof(TResponse);
        var createMethod = typeof(Result<>).MakeGenericType(resultType.GetGenericArguments())
          .GetMethod("Error", new[] { typeof(string[]) });

        return (TResponse)createMethod?.Invoke(null, new object[] { errors })!;
      }

      throw;
    }
    catch (Exception)
    {
      // Log the exception and return generic error if TResponse is Result<T>
      if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
      {
        var errors = new[] { "An unexpected error occurred" };
        var resultType = typeof(TResponse);
        var createMethod = typeof(Result<>).MakeGenericType(resultType.GetGenericArguments())
          .GetMethod("Error", new[] { typeof(string[]) });

        return (TResponse)createMethod?.Invoke(null, new object[] { errors })!;
      }

      throw;
    }
  }
}
