using Server.Core.TagAggregate;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Tags.List;

public class ListTagsHandler(IReadRepository<Tag> tagRepo)
  : IQueryHandler<ListTagsQuery, List<string>>
{
  public async Task<Result<List<string>>> Handle(ListTagsQuery request, CancellationToken cancellationToken)
  {
    var tags = await tagRepo.ListAsync(cancellationToken);
    var tagNames = tags.Select(t => t.Name).ToList();
    return Result<List<string>>.Success(tagNames);
  }
}
