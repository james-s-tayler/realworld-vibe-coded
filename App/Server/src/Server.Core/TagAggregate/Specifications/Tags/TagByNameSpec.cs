namespace Server.Core.TagAggregate.Specifications;

public class TagByNameSpec : Specification<Tag>
{
  public TagByNameSpec(string name)
  {
    Query.Where(x => x.Name == name);
  }
}
