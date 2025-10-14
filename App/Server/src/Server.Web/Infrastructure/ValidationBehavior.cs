using FluentValidation;

namespace Server.Web.Infrastructure;

/// <summary>
/// MediatR pipeline behavior that validates commands and queries using FluentValidation.
/// </summary>
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

    var validationResults = await Task.WhenAll(
      _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

    var failures = validationResults
      .Where(r => !r.IsValid)
      .SelectMany(r => r.Errors)
      .ToList();

    if (failures.Count != 0)
    {
      throw new FluentValidation.ValidationException(failures);
    }

    return await next();
  }
}
