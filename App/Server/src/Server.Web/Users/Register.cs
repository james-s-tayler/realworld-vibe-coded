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
      await SendCreatedAtAsync<Register>(new { }, Response, cancellation: cancellationToken);
      return;
    }

    if (result.IsInvalid())
    {
      var errors = new Dictionary<string, string[]>();
      foreach (var error in result.ValidationErrors)
      {
        errors[error.Identifier] = new[] { error.ErrorMessage };
      }
      await SendResultAsync(Results.UnprocessableEntity(new { errors }));
      return;
    }

    await SendResultAsync(Results.BadRequest(new { errors = new { message = new[] { result.Errors.FirstOrDefault() ?? "Registration failed" } } }));
  }
}