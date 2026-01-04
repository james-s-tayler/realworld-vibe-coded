using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.Core.AuthorAggregate;
using Server.Core.IdentityAggregate;
using Server.Infrastructure;
using Server.Infrastructure.Data;
using Server.UseCases.Identity.Register;

namespace Server.Web.Identity.Register;

public class Register(IMediator mediator, AppDbContext dbContext, UserManager<ApplicationUser> userManager) : Endpoint<RegisterRequest>
{
  public override void Configure()
  {
    Post("/api/identity/register");
    AllowAnonymous();
  }

  public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
  {
    var command = new RegisterCommand
    {
      Email = req.Email,
      Password = req.Password,
    };
    var result = await mediator.Send(command, ct);

    if (result.IsSuccess)
    {
      // Create Author record as domain invariant after successful user registration
      // Use UserManager to find user, which handles tenant context properly
      var user = await userManager.FindByEmailAsync(req.Email);
      if (user != null && user.UserName != null)
      {
        var author = new Author(user.Id, user.UserName, user.Bio ?? string.Empty, user.Image);
        dbContext.Authors.Add(author);
        await dbContext.SaveChangesAsync(ct);
      }
    }

    await Send.ResultValueAsync(result, ct);
  }
}
