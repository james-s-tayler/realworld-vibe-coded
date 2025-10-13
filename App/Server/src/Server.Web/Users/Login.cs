using Server.UseCases.Users.Login;

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
      HttpContext.Response.StatusCode = 401;
      HttpContext.Response.ContentType = "application/json";
      var json = System.Text.Json.JsonSerializer.Serialize(new
      {
        errors = new { body = new[] { "email or password is invalid" } }
      });
      await HttpContext.Response.WriteAsync(json, cancellationToken);
      return;
    }

    HttpContext.Response.StatusCode = 400;
    HttpContext.Response.ContentType = "application/json";
    var errorJson = System.Text.Json.JsonSerializer.Serialize(new
    {
      errors = new { body = new[] { result.Errors.FirstOrDefault() ?? "Login failed" } }
    });
    await HttpContext.Response.WriteAsync(errorJson, cancellationToken);
  }
}
