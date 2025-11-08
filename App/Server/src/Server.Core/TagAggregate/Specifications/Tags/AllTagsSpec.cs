namespace Server.Core.TagAggregate.Specifications;

public class AllTagsSpec : Specification<Tag>
{
  public AllTagsSpec()
  {
    Query.AsNoTracking();
  }
}
