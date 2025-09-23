using Ardalis.Specification;

namespace Server.Core.TagAggregate;

public class AllTagsSpec : Specification<Tag>
{
  public AllTagsSpec()
  {
    Query.OrderBy(tag => tag.Name);
  }
}
