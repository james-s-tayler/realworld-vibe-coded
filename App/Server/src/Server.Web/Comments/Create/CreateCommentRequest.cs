namespace Server.Web.Comments.Create;

public class CreateCommentRequest
{
  [RouteParam]
  public string Slug { get; set; } = string.Empty;

  public CreateCommentData Comment { get; set; } = new();
}
