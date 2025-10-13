# FastEndpoints v7.0 Migration Guide

## Overview

This document describes the migration from FastEndpoints 6.2.0 to 7.0.1.

## Package Updates

The following packages were updated:

| Package | Old Version | New Version |
|---------|-------------|-------------|
| FastEndpoints | 6.2.0 | 7.0.1 |
| FastEndpoints.Swagger | 6.2.0 | 7.0.1 |
| FastEndpoints.ApiExplorer | 2.2.0 | 2.3.0 |
| FastEndpoints.Swagger.Swashbuckle | 2.2.0 | 2.3.0 |

## Breaking Changes

### 1. Removal of `SendNotFoundAsync()` and `SendNoContentAsync()` Methods

**Impact:** These helper methods were removed from the base Endpoint class.

**Migration:**

#### Before (v6.2.0):
```csharp
public override async Task HandleAsync(GetContributorByIdRequest request,
    CancellationToken cancellationToken)
{
    var result = await _mediator.Send(query, cancellationToken);

    if (result.Status == ResultStatus.NotFound)
    {
        await SendNotFoundAsync(cancellationToken);
        return;
    }

    if (result.IsSuccess)
    {
        Response = new ContributorRecord(result.Value.Id, result.Value.Name, result.Value.PhoneNumber);
    }
}
```

#### After (v7.0.1):
```csharp
public override async Task HandleAsync(GetContributorByIdRequest request,
    CancellationToken cancellationToken)
{
    var result = await _mediator.Send(query, cancellationToken);

    if (result.Status == ResultStatus.NotFound)
    {
        HttpContext.Response.StatusCode = 404;
        HttpContext.Response.ContentType = "application/json";
        var notFoundJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            errors = new { body = new[] { "Contributor not found" } }
        });
        await HttpContext.Response.WriteAsync(notFoundJson, cancellationToken);
        return;
    }

    if (result.IsSuccess)
    {
        Response = new ContributorRecord(result.Value.Id, result.Value.Name, result.Value.PhoneNumber);
    }
}
```

**Key Changes:**
1. Replace `await SendNotFoundAsync(cancellationToken)` with:
   - `HttpContext.Response.StatusCode = 404`
   - Set `ContentType` to `"application/json"`
   - Serialize and write error response body

2. Replace `await SendNoContentAsync(cancellationToken)` with:
   - `HttpContext.Response.StatusCode = 204`

**Important:** In FastEndpoints v7, endpoints that set a status code but don't write a response body may get their status code overridden. Always provide a response body for error responses.

## Files Modified

The following endpoints were updated to accommodate the breaking changes:

- `App/Server/src/Server.Web/Contributors/GetById.cs`
- `App/Server/src/Server.Web/Contributors/Update.cs`
- `App/Server/src/Server.Web/Contributors/Delete.cs`

## Testing

All tests pass after migration:
- ✅ Unit Tests
- ✅ Integration Tests
- ✅ Functional Tests
- ✅ Postman API Tests

## References

- [FastEndpoints Documentation](https://fast-endpoints.com/)
- [FastEndpoints GitHub Repository](https://github.com/FastEndpoints/FastEndpoints)
