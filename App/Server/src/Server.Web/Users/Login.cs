using Server.UseCases.Users.Login;
using Server.Web.Infrastructure;

namespace Server.Web.Users;

/// <summary>
/// Login user
/// </summary>
/// <remarks>
/// Authenticate user with email and password. Returns user details with JWT token.
/// </remarks>
public class Login(IMediator _mediator) : Endpoint<LoginRequest, LoginResponse>
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

  public override void OnValidationFailed()
  {
    var errorBody = new List<string>();

    foreach (var failure in ValidationFailures)
    {
      errorBody.Add($"{failure.PropertyName.ToLower()} {failure.ErrorMessage}");
    }

    HttpContext.Response.SendAsync(new ConduitErrorResponse
    {
      Errors = new ConduitErrorBody { Body = errorBody.ToArray() }
    }, 422).GetAwaiter().GetResult();
  }

  public override async Task HandleAsync(
    LoginRequest request,
    CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new LoginUserQuery(
      request.User.Email,
      request.User.Password), cancellationToken);

    if (result.IsSuccess)
    {
      var userDto = result.Value;
      Response = new LoginResponse
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

    if (result.Status == ResultStatus.Unauthorized)
    {
      await HttpContext.Response.HttpContext.Response.SendAsync(new ConduitErrorResponse
      {
        Errors = new ConduitErrorBody { Body = new[] { "email or password is invalid" } }
      }, 401);
      return;
    }

    await HttpContext.Response.HttpContext.Response.SendAsync(new ConduitErrorResponse
    {
      Errors = new ConduitErrorBody { Body = new[] { result.Errors.FirstOrDefault() ?? "Login failed" } }
    }, 400);
  }
}
