namespace Server.Web.Comments.Delete;

public class DeleteCommentRequest
{
  [RouteParam]
  public string Slug { get; set; } = string.Empty;

  [RouteParam]
  public string Id { get; set; } = string.Empty;
}
