using Server.Infrastructure;
using Server.UseCases.Users.Register;

namespace Server.Web.Users.Register;

/// <summary>
/// Register a new user
/// </summary>
/// <remarks>
/// Creates a new user account given email, username, and password.
/// </remarks>
public class Register(IMediator mediator) : Endpoint<RegisterRequest, RegisterResponse, UserMapper>
{
  public override void Configure()
  {
    Post(RegisterRequest.Route);
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Register a new user";
      s.Description = "Create a new user account. Returns user details with JWT token.";
      s.ExampleRequest = new RegisterRequest
      {
        User = new UserData
        {
          Email = "user@example.com",
          Username = "username",
          Password = "password123",
        },
      };
    });
  }

  public override async Task HandleAsync(
    RegisterRequest request,
    CancellationToken cancellationToken)
  {
    // Default username to email if not provided
    var username = string.IsNullOrEmpty(request.User.Username)
      ? request.User.Email
      : request.User.Username;

    var result = await mediator.Send(
      new RegisterUserCommand(
        request.User.Email,
        username,
        request.User.Password),
      cancellationToken);

    await Send.ResultMapperAsync(
      result,
      userDto => new RegisterResponse
      {
        User = Map.FromEntity(userDto),
      },
      cancellationToken);
  }
}
