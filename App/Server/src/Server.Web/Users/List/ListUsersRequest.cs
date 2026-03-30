namespace Server.Web.Users.List;

public class ListUsersRequest
{
  [QueryParam]
  public int Limit { get; set; } = 20;

  [QueryParam]
  public int Offset { get; set; } = 0;
}
