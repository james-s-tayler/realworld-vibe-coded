using Server.Core.Interfaces;
using Server.Infrastructure;
using Server.UseCases.Users.Update;

namespace Server.Web.Users.Update;

/// <summary>
/// Update current user
/// </summary>
/// <remarks>
/// Update the currently authenticated user's details.
/// </remarks>
public class UpdateUser(IMediator _mediator, IUserContext userContext) : Endpoint<UpdateUserRequest, UpdateUserResponse, UserMapper>
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

  public override async Task HandleAsync(
    UpdateUserRequest request,
    CancellationToken cancellationToken)
  {
    var userId = userContext.GetRequiredCurrentUserId();

    var result = await _mediator.Send(new UpdateUserCommand(
      userId,
      request.User.Email,
      request.User.Username,
      request.User.Password,
      request.User.Bio,
      request.User.Image), cancellationToken);

    await Send.ResultMapperAsync(result, userDto => new UpdateUserResponse
    {
      User = Map.FromEntity(userDto)
    }, cancellationToken);
  }
}
