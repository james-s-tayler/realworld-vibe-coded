using Destructurama.Attributed;

namespace Server.Web.Identity.Invite;

public class InviteRequest
{
  public string Email { get; set; } = default!;

  [NotLogged]
  public string Password { get; set; } = default!;
}
