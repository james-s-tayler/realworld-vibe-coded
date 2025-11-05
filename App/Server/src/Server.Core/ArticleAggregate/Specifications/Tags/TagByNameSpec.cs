namespace Server.Core.ArticleAggregate.Specifications.Tags;

public class TagByNameSpec : Specification<Tag>
{
  public TagByNameSpec(string name)
  {
    Query.Where(x => x.Name == name);
  }
}
