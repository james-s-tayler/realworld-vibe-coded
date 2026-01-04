using Server.Infrastructure;
using Server.UseCases.Interfaces;
using Server.UseCases.Users.GetCurrent;

namespace Server.Web.Users.GetCurrent;

public class GetCurrent(IMediator mediator, IUserContext userContext) : Endpoint<EmptyRequest, UserCurrentResponse>
{
  public override void Configure()
  {
    Get("/api/user");
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
    Summary(s =>
    {
      s.Summary = "Get current user";
      s.Description = "Get the currently authenticated user details.";
    });
  }

  public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
  {
    var userId = userContext.GetRequiredCurrentUserId();

    var result = await mediator.Send(new GetCurrentUserQuery(userId), cancellationToken);

    await Send.ResultMapperAsync(
      result,
      async (userDto, ct) =>
      {
        return new UserCurrentResponse
        {
          User = new UserResponse
          {
            Email = userDto.Email,
            Username = userDto.Username,
            Bio = userDto.Bio,
            Image = userDto.Image,
            Roles = userDto.Roles,
          },
        };
      },
      cancellationToken);
  }
}
