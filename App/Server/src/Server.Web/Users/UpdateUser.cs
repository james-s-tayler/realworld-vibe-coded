using Server.Core.Interfaces;
using Server.UseCases.Users.Update;
using Server.Web.Infrastructure;

namespace Server.Web.Users;

/// <summary>
/// Update current user
/// </summary>
/// <remarks>
/// Update the currently authenticated user's details.
/// </remarks>
public class UpdateUser(IMediator _mediator, ICurrentUserService _currentUserService) : Endpoint<UpdateUserRequest, UpdateUserResponse>
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

    Send.ResponseAsync<ConduitErrorResponse>(new ConduitErrorResponse
    {
      Errors = new ConduitErrorBody { Body = errorBody.ToArray() }
    }, 422).GetAwaiter().GetResult();
  }

  public override async Task HandleAsync(
    UpdateUserRequest request,
    CancellationToken cancellationToken)
  {
    var userId = _currentUserService.GetRequiredCurrentUserId();

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

      await Send.ResponseAsync<ConduitErrorResponse>(new ConduitErrorResponse
      {
        Errors = new ConduitErrorBody { Body = errorBody.ToArray() }
      }, 422);
      return;
    }

    if (result.Status == ResultStatus.NotFound)
    {
      await Send.ResponseAsync<ConduitErrorResponse>(new ConduitErrorResponse
      {
        Errors = new ConduitErrorBody { Body = new[] { "Unauthorized" } }
      }, 401);
      return;
    }

    await Send.ResponseAsync<ConduitErrorResponse>(new ConduitErrorResponse
    {
      Errors = new ConduitErrorBody { Body = new[] { result.Errors.FirstOrDefault() ?? "Update failed" } }
    }, 400);
  }
}
