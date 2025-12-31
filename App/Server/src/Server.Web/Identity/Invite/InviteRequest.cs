namespace Server.Web.Identity.Invite;

public class InviteRequest
{
  public string Email { get; set; } = default!;

  public string Password { get; set; } = default!;
}
