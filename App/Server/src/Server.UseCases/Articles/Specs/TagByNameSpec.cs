using Ardalis.Specification;
using Server.Core.TagAggregate;

namespace Server.UseCases.Articles.Specs;

public class TagByNameSpec : Specification<Tag>
{
  public TagByNameSpec(string name)
  {
    Query.Where(t => t.Name == name);
  }
}
