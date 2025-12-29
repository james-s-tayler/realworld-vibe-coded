using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.Infrastructure;
using Server.Infrastructure.Data;
using Server.UseCases.Interfaces;
using Server.UseCases.Users.Update;

namespace Server.Web.Users.Update;

/// <summary>
/// Update current user
/// </summary>
/// <remarks>
/// Update the currently authenticated user's details.
/// </remarks>
public class UpdateUser(IMediator mediator, IUserContext userContext, UserManager<Core.IdentityAggregate.ApplicationUser> userManager, AppDbContext dbContext) : Endpoint<UpdateUserRequest, UpdateUserResponse>
{
  public override void Configure()
  {
    Put(UpdateUserRequest.Route);
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
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
          Image = "https://example.com/avatar.jpg",
        },
      };
    });
  }

  public override async Task HandleAsync(
    UpdateUserRequest request,
    CancellationToken cancellationToken)
  {
    var userId = userContext.GetRequiredCurrentUserId();

    // Check for duplicate email before calling handler
    if (!string.IsNullOrEmpty(request.User.Email))
    {
      var normalizedEmail = userManager.NormalizeEmail(request.User.Email);
      var existingUserByEmail = await dbContext.Users
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail && u.Id != userId, cancellationToken);

      if (existingUserByEmail != null)
      {
        await HttpContext.Response.SendErrorsAsync(
          new List<ValidationFailure>
          {
            new ValidationFailure("email", "Email already exists"),
          },
          statusCode: 400,
          cancellation: cancellationToken);
        return;
      }
    }

    // Check for duplicate username before calling handler
    if (!string.IsNullOrEmpty(request.User.Username))
    {
      var normalizedUserName = userManager.NormalizeName(request.User.Username);
      var existingUserByUsername = await dbContext.Users
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName && u.Id != userId, cancellationToken);

      if (existingUserByUsername != null)
      {
        await HttpContext.Response.SendErrorsAsync(
          new List<ValidationFailure>
          {
            new ValidationFailure("username", "Username already exists"),
          },
          statusCode: 400,
          cancellation: cancellationToken);
        return;
      }
    }

    var result = await mediator.Send(
      new UpdateUserCommand(
        userId,
        request.User.Email,
        request.User.Username,
        request.User.Password,
        request.User.Bio,
        request.User.Image),
      cancellationToken);

    if (result.IsSuccess)
    {
      await dbContext.SaveChangesAsync(cancellationToken);
    }

    await Send.ResultMapperAsync(
      result,
      user => new UpdateUserResponse
      {
        User = new UserResponse
        {
          Email = user.Email!,
          Username = user.UserName!,
          Bio = user.Bio ?? string.Empty,
          Image = user.Image,
        },
      },
      cancellationToken);
  }
}
