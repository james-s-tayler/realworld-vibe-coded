using Server.UseCases.Users.Login;
using Server.Web.Infrastructure;

namespace Server.Web.Users;

/// <summary>
/// Login user
/// </summary>
/// <remarks>
/// Authenticate user with email and password. Returns user details with JWT token.
/// </remarks>
public class Login(IMediator _mediator) : Endpoint<LoginRequest, LoginResponse, UserMapper>
{
  public override void Configure()
  {
    Post(LoginRequest.Route);
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Login user";
      s.Description = "Authenticate user with email and password. Returns user details with JWT token.";
      s.ExampleRequest = new LoginRequest
      {
        User = new LoginUserData
        {
          Email = "user@example.com",
          Password = "password123"
        }
      };
    });
  }

  public override async Task HandleAsync(
    LoginRequest request,
    CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new LoginUserQuery(
      request.User.Email,
      request.User.Password), cancellationToken);

    await Send.ResultAsync(result, userDto => new LoginResponse
    {
      User = Map.FromEntity(userDto)
    }, cancellationToken);
  }
}
