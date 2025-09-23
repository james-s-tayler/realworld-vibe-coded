using System.Security.Claims;
using Server.UseCases.Users.Update;

namespace Server.Web.Users;

/// <summary>
/// Update current user
/// </summary>
/// <remarks>
/// Update the currently authenticated user's details.
/// </remarks>
public class UpdateUser(IMediator _mediator) : Endpoint<UpdateUserRequest, UpdateUserResponse>
{
  public override void Configure()
  {
    Put(UpdateUserRequest.Route);
    AuthSchemes("Token");
    Summary(s =>
    {
      s.Summary = "Update current user";
      s.Description = "Update the currently authenticated user's details.";
      s.ExampleRequest = new UpdateUserRequest
      {
        User = new UpdateUserData
        {
          Email = "updated@example.com",
          Username = "newusername",
          Bio = "Updated bio",
          Image = "https://example.com/avatar.jpg"
        }
      };
    });
  }

  public override void OnValidationFailed()
  {
    var errorBody = new List<string>();

    foreach (var failure in ValidationFailures)
    {
      errorBody.Add($"{failure.PropertyName.ToLower()} {failure.ErrorMessage}");
    }

    HttpContext.Response.StatusCode = 422;
    HttpContext.Response.ContentType = "application/json";
    var json = System.Text.Json.JsonSerializer.Serialize(new
    {
      errors = new { body = errorBody }
    });
    HttpContext.Response.WriteAsync(json).GetAwaiter().GetResult();
  }

  public override async Task HandleAsync(
    UpdateUserRequest request,
    CancellationToken cancellationToken)
  {
    var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
    {
      HttpContext.Response.StatusCode = 401;
      HttpContext.Response.ContentType = "application/json";
      var unauthorizedJson = System.Text.Json.JsonSerializer.Serialize(new
      {
        errors = new { body = new[] { "Unauthorized" } }
      });
      await HttpContext.Response.WriteAsync(unauthorizedJson, cancellationToken);
      return;
    }

    var result = await _mediator.Send(new UpdateUserCommand(
      userId,
      request.User.Email,
      request.User.Username,
      request.User.Password,
      request.User.Bio,
      request.User.Image), cancellationToken);

    if (result.IsSuccess)
    {
      var userDto = result.Value;
      Response = new UpdateUserResponse
      {
        User = new UserResponse
        {
          Email = userDto.Email,
          Username = userDto.Username,
          Bio = userDto.Bio,
          Image = userDto.Image,
          Token = userDto.Token
        }
      };
      return;
    }

    if (result.IsInvalid())
    {
      var errorBody = new List<string>();
      foreach (var error in result.ValidationErrors)
      {
        errorBody.Add($"{error.Identifier} {error.ErrorMessage}");
      }

      HttpContext.Response.StatusCode = 422;
      HttpContext.Response.ContentType = "application/json";
      var validationJson = System.Text.Json.JsonSerializer.Serialize(new
      {
        errors = new { body = errorBody }
      });
      await HttpContext.Response.WriteAsync(validationJson, cancellationToken);
      return;
    }

    if (result.Status == ResultStatus.NotFound)
    {
      HttpContext.Response.StatusCode = 401;
      HttpContext.Response.ContentType = "application/json";
      var notFoundJson = System.Text.Json.JsonSerializer.Serialize(new
      {
        errors = new { body = new[] { "Unauthorized" } }
      });
      await HttpContext.Response.WriteAsync(notFoundJson, cancellationToken);
      return;
    }

    HttpContext.Response.StatusCode = 400;
    HttpContext.Response.ContentType = "application/json";
    var errorJson = System.Text.Json.JsonSerializer.Serialize(new
    {
      errors = new { body = new[] { result.Errors.FirstOrDefault() ?? "Update failed" } }
    });
    await HttpContext.Response.WriteAsync(errorJson, cancellationToken);
  }
}
