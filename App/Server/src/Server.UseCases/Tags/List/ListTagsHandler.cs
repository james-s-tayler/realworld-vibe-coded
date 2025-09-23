using Ardalis.Result;
using Ardalis.SharedKernel;
using Microsoft.Extensions.Logging;
using Server.Core.Interfaces;
using Server.Core.TagAggregate;

namespace Server.UseCases.Tags.List;

public class ListTagsHandler(
  IRepository<Tag> _tagRepository,
  ILogger<ListTagsHandler> _logger)
  : IQueryHandler<ListTagsQuery, Result<ListTagsResult>>
{
  public async Task<Result<ListTagsResult>> Handle(
    ListTagsQuery request,
    CancellationToken cancellationToken)
  {
    _logger.LogInformation("Handling {Query}", nameof(ListTagsQuery));

    try
    {
      var tags = await _tagRepository
        .ListAsync(new AllTagsSpec(), cancellationToken);

      var result = new ListTagsResult
      {
        Tags = tags.Select(t => t.Name).ToList()
      };

      _logger.LogInformation("Found {TagCount} tags", tags.Count);
      return Result.Success(result);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error handling {Query}", nameof(ListTagsQuery));
      return Result.Error("An error occurred while retrieving tags");
    }
  }
}
