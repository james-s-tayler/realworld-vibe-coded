using Server.UseCases.Users.Register;

namespace Server.Web.Users;

/// <summary>
/// Register a new user
/// </summary>
/// <remarks>
/// Creates a new user account given email, username, and password.
/// </remarks>
public class Register(IMediator _mediator) : Endpoint<RegisterRequest, RegisterResponse>
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
          Password = "password123"
        }
      };
    });
  }

  public override async Task HandleAsync(
    RegisterRequest request,
    CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new RegisterUserCommand(
      request.User.Email,
      request.User.Username,
      request.User.Password), cancellationToken);

    if (result.IsSuccess)
    {
      var userDto = result.Value;
      await SendAsync(new RegisterResponse
      {
        User = new UserResponse
        {
          Email = userDto.Email,
          Username = userDto.Username,
          Bio = userDto.Bio,
          Image = userDto.Image,
          Token = userDto.Token
        }
      }, 201, cancellationToken);
      return;
    }

    if (result.IsInvalid())
    {
      var errorBody = new List<string>();
      foreach (var error in result.ValidationErrors)
      {
        errorBody.Add($"{error.Identifier} {error.ErrorMessage}");
      }

      await SendAsync(new
      {
        errors = new { body = errorBody }
      }, 422, cancellationToken);
      return;
    }

    await SendAsync(new
    {
      errors = new { body = new[] { result.Errors.FirstOrDefault() ?? "Registration failed" } }
    }, 400, cancellationToken);
  }
}
