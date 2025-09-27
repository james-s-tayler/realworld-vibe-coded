using Ardalis.Result;
using FastEndpoints;
using FluentValidation;

namespace Server.Web.Infrastructure;

/// <summary>
/// FastEndpoints global error handler that provides consistent error responses
/// </summary>
public class GlobalErrorHandler : IGlobalPostProcessor
{
  public async Task PostProcessAsync(IPostProcessorContext context, CancellationToken ct)
  {
    if (context.HasExceptionOccurred)
    {
      var exception = context.ExceptionDispatchInfo?.SourceException;
      
      switch (exception)
      {
        case ValidationException validationException:
          context.HttpContext.Response.StatusCode = 422;
          context.HttpContext.Response.ContentType = "application/json";
          
          var validationResponse = ErrorResponseBuilder.CreateValidationErrorResponse(validationException.Errors);
          await context.HttpContext.Response.WriteAsync(validationResponse, ct);
          break;

        case UnauthorizedAccessException:
          context.HttpContext.Response.StatusCode = 401;
          context.HttpContext.Response.ContentType = "application/json";
          
          var unauthorizedResponse = ErrorResponseBuilder.CreateUnauthorizedResponse();
          await context.HttpContext.Response.WriteAsync(unauthorizedResponse, ct);
          break;

        case ArgumentException argumentException:
          context.HttpContext.Response.StatusCode = 400;
          context.HttpContext.Response.ContentType = "application/json";
          
          var argumentResponse = ErrorResponseBuilder.CreateErrorResponse(argumentException.Message);
          await context.HttpContext.Response.WriteAsync(argumentResponse, ct);
          break;

        default:
          context.HttpContext.Response.StatusCode = 500;
          context.HttpContext.Response.ContentType = "application/json";
          
          var errorResponse = ErrorResponseBuilder.CreateErrorResponse("An unexpected error occurred");
          await context.HttpContext.Response.WriteAsync(errorResponse, ct);
          break;
      }
    }
  }
}