namespace Server.Core.Interfaces;

public interface IListTagsQueryService
{
  Task<IEnumerable<string>> ListAsync();
}
