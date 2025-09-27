# Centralized Error Handling Documentation

This document describes the centralized error and validation handling system implemented for the RealWorld API backend.

## Overview

The error handling system provides consistent, centralized error responses across all API endpoints. It replaces duplicated error handling code with a unified approach using MediatR pipeline behaviors and FastEndpoints infrastructure.

## Key Components

### 1. ErrorResponseBuilder

Central utility for creating RealWorld API-compliant error responses.

**Location**: `Server.Web/Infrastructure/ErrorResponseBuilder.cs`

**Key Methods**:
- `CreateValidationErrorResponse(IEnumerable<ValidationFailure>)` - For FluentValidation errors
- `CreateValidationErrorResponse(IEnumerable<ValidationError>)` - For Ardalis.Result validation errors  
- `CreateErrorResponse(string)` - For single error messages
- `CreateErrorResponse(IEnumerable<string>)` - For multiple error messages
- `CreateUnauthorizedResponse()` - For 401 unauthorized errors

**Response Format**:
```json
{
  "errors": {
    "body": ["error message 1", "error message 2"]
  }
}
```

### 2. MediatR Pipeline Behaviors

#### ValidationBehavior<TRequest, TResponse>
- Intercepts all MediatR requests to run FluentValidation
- Converts validation failures to Ardalis.Result errors
- **Location**: `Server.Web/Infrastructure/ValidationBehavior.cs`

#### ErrorHandlingBehavior<TRequest, TResponse>  
- Catches exceptions from MediatR handlers
- Converts exceptions to appropriate Ardalis.Result responses
- **Location**: `Server.Web/Infrastructure/ErrorHandlingBehavior.cs`

**Supported Exception Mappings**:
- `ValidationException` → `Result.Invalid()`
- `UnauthorizedAccessException` → `Result.Unauthorized()`
- `ArgumentException` → `Result.Error()`
- Generic exceptions → `Result.Error("An unexpected error occurred")`

### 3. BaseResultEndpoint<TRequest, TResponse>

Base class for FastEndpoints that provides standardized Result<T> handling.

**Location**: `Server.Web/Infrastructure/BaseResultEndpoint.cs`

**Key Features**:
- Automatic HTTP status code mapping from Ardalis.Result statuses
- Centralized error response formatting
- Helper methods for common operations (GetCurrentUserId, HandleResultAsync)

**Status Code Mappings**:
- `ResultStatus.Invalid` → HTTP 422 (Unprocessable Entity)
- `ResultStatus.Unauthorized` → HTTP 401 (Unauthorized)  
- `ResultStatus.Forbidden` → HTTP 403 (Forbidden)
- `ResultStatus.NotFound` → HTTP 404 (Not Found)
- `ResultStatus.Conflict` → HTTP 409 (Conflict)
- `ResultStatus.Error` → HTTP 422 (Unprocessable Entity) - for RealWorld API compliance

### 4. Configuration

The pipeline behaviors are registered in `MediatrConfigs.cs`:

```csharp
services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(mediatRAssemblies!))
        .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
        .AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))
        .AddScoped(typeof(IPipelineBehavior<,>), typeof(ErrorHandlingBehavior<,>));
```

## Usage Patterns

### For New Endpoints

1. Inherit from `BaseResultEndpoint<TRequest, TResponse>`
2. Use `HandleResultAsync()` to process Ardalis.Result responses
3. Throw appropriate exceptions for error conditions

```csharp
public class Create(IMediator _mediator) : BaseResultEndpoint<CreateArticleRequest, ArticleResponse>
{
  public override async Task HandleAsync(CreateArticleRequest request, CancellationToken cancellationToken)
  {
    var userId = GetCurrentUserId();
    if (userId == null)
    {
      throw new UnauthorizedAccessException(); // Automatically handled
    }

    var result = await _mediator.Send(new CreateArticleCommand(...), cancellationToken);
    await HandleResultAsync(result, 201, cancellationToken); // Automatic status mapping
  }
}
```

### For MediatR Handlers

Return appropriate Ardalis.Result statuses:

```csharp
public async Task<Result<ArticleResponse>> Handle(CreateArticleCommand request, CancellationToken cancellationToken)
{
  // Validation errors are handled automatically by ValidationBehavior
  
  if (slugExists)
  {
    return Result.Error("Article with this title already exists");
  }
  
  // Success case
  return Result.Success(articleResponse);
}
```

## Benefits

1. **Consistency**: All endpoints return errors in the same RealWorld API format
2. **Reduced Duplication**: No more copy/pasted error handling code
3. **Type Safety**: Compile-time checking of error response formats
4. **Separation of Concerns**: Business logic focus in handlers, error formatting in infrastructure
5. **Maintainability**: Single place to update error response format

## Testing

The system includes comprehensive tests:
- `ErrorResponseBuilderTests` - Unit tests for response formatting
- Postman API tests validate all 562 assertions across 139 requests
- Integration tests ensure end-to-end error handling works correctly

## Migration from Old Pattern

**Before** (Manual error handling):
```csharp
if (result.IsInvalid())
{
  var errorBody = new List<string>();
  foreach (var error in result.ValidationErrors)
  {
    errorBody.Add($"{error.Identifier} {error.ErrorMessage}");
  }
  HttpContext.Response.StatusCode = 422;
  HttpContext.Response.ContentType = "application/json";
  var json = System.Text.Json.JsonSerializer.Serialize(new { errors = new { body = errorBody } });
  await HttpContext.Response.WriteAsync(json, cancellationToken);
  return;
}
```

**After** (Centralized handling):
```csharp
await HandleResultAsync(result, 201, cancellationToken);
```

The new approach reduces ~15-20 lines of boilerplate per endpoint to a single line.