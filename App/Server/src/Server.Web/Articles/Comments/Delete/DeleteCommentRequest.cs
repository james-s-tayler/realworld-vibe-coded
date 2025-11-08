namespace Server.Web.Articles.Comments.Delete;

public class DeleteCommentRequest
{
  public Guid Id { get; set; }

  public string Slug { get; set; } = String.Empty;
}
