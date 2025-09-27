using Ardalis.Result;
using FluentValidation;
using MediatR;

namespace Server.Web.Infrastructure;

/// <summary>
/// Pipeline behavior that validates requests using FluentValidation
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IRequest<TResponse>
{
  private readonly IEnumerable<IValidator<TRequest>> _validators;

  public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
  {
    _validators = validators;
  }

  public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
  {
    if (!_validators.Any())
    {
      return await next();
    }

    var context = new FluentValidation.ValidationContext<TRequest>(request);
    var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
    
    var failures = validationResults
      .SelectMany(r => r.Errors)
      .Where(f => f != null)
      .ToList();

    if (failures.Any())
    {
      // If TResponse is Result<T>, return validation errors in Result format
      if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
      {
        var validationErrors = failures.Select(f => new ValidationError
        {
          Identifier = f.PropertyName,
          ErrorMessage = f.ErrorMessage
        }).ToList();

        var invalidResult = Result.Invalid(validationErrors);
        var resultType = typeof(TResponse);
        var createMethod = typeof(Result<>).MakeGenericType(resultType.GetGenericArguments())
          .GetMethod("Invalid", new[] { typeof(List<ValidationError>) });
        
        return (TResponse)createMethod?.Invoke(null, new object[] { validationErrors })!;
      }

      // For non-Result types, throw validation exception that will be caught by error handling behavior
      throw new ValidationException(failures);
    }

    return await next();
  }
}