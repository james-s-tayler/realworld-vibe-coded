namespace Server.Web.Articles.Comments;

public class DeleteCommentRequest
{
  public string Slug { get; set; } = string.Empty;
  public int Id { get; set; }
}
