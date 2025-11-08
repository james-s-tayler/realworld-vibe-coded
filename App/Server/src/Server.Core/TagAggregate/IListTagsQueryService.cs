namespace Server.Core.TagAggregate;

public interface IListTagsQueryService
{
  Task<IEnumerable<string>> ListAsync();
}
