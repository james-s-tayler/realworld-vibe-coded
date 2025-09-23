using Ardalis.Result;
using Ardalis.SharedKernel;

namespace Server.UseCases.Tags.List;

public record ListTagsQuery() : IQuery<Result<ListTagsResult>>;

public class ListTagsResult
{
  public List<string> Tags { get; set; } = new();
}
