namespace Server.Web.Tags;

public class TagsMapper : ResponseMapper<TagsResponse, List<string>>
{
  public override Task<TagsResponse> FromEntityAsync(List<string> tags, CancellationToken ct)
  {
    return Task.FromResult(new TagsResponse { Tags = tags });
  }
}
