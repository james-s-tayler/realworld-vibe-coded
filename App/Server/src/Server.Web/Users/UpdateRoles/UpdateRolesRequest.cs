namespace Server.Web.Users.UpdateRoles;

public class UpdateRolesRequest
{
  public Guid UserId { get; set; }

  public List<string> Roles { get; set; } = [];
}
