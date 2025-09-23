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
      Response = new RegisterResponse
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
      HttpContext.Response.StatusCode = 201;
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
      var json = System.Text.Json.JsonSerializer.Serialize(new
      {
        errors = new { body = errorBody }
      });
      await HttpContext.Response.WriteAsync(json, cancellationToken);
      return;
    }

    HttpContext.Response.StatusCode = 400;
    HttpContext.Response.ContentType = "application/json";
    var errorJson = System.Text.Json.JsonSerializer.Serialize(new
    {
      errors = new { body = new[] { result.Errors.FirstOrDefault() ?? "Registration failed" } }
    });
    await HttpContext.Response.WriteAsync(errorJson, cancellationToken);
  }
}