namespace Server.Web.Tags;

public class ListTagsRequest
{
  public const string Route = "/api/tags";
}

public class ListTagsResponse
{
  public List<string> Tags { get; set; } = new();
}
