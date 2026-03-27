using Server.Web.Profiles;

namespace Server.Web.Comments;

public class CommentDto
{
  public Guid Id { get; set; }

  public string Body { get; set; } = string.Empty;

  public DateTime CreatedAt { get; set; }

  public DateTime UpdatedAt { get; set; }

  public ProfileDto Author { get; set; } = default!;
}
