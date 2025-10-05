namespace Server.Web.Contributors;

public class GetContributorByIdRequest
{
  public const string Route = "/api/contributors/{ContributorId:int}";
  public static string BuildRoute(int contributorId) => Route.Replace("{ContributorId:int}", contributorId.ToString());

  public int ContributorId { get; set; }
}
