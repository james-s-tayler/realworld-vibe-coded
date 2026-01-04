using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.Core.AuthorAggregate;
using Server.Core.IdentityAggregate;
using Server.Infrastructure;
using Server.Infrastructure.Data;
using Server.UseCases.Identity.Invite;

namespace Server.Web.Identity.Invite;

public class Invite(IMediator mediator, AppDbContext dbContext, UserManager<ApplicationUser> userManager) : Endpoint<InviteRequest>
{
  public override void Configure()
  {
    Post("/api/identity/invite");
    AuthSchemes(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme, Microsoft.AspNetCore.Identity.IdentityConstants.BearerScheme);
    Roles(Server.Core.IdentityAggregate.DefaultRoles.Admin);
  }

  public override async Task HandleAsync(InviteRequest req, CancellationToken ct)
  {
    var command = new InviteCommand(req.Email, req.Password);
    var result = await mediator.Send(command, ct);

    if (result.IsSuccess)
    {
      // Create Author record as domain invariant after successful user invitation
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
