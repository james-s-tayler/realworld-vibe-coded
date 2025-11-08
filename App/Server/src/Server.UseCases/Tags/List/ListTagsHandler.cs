using Server.Core.TagAggregate;
using Server.Core.TagAggregate.Specifications;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Tags.List;

public class ListTagsHandler(IReadRepository<Tag> tagRepository)
  : IQueryHandler<ListTagsQuery, TagsResponse>
{
  public async Task<Result<TagsResponse>> Handle(ListTagsQuery request, CancellationToken cancellationToken)
  {
    var tags = await tagRepository.ListAsync(new AllTagsSpec(), cancellationToken);
    var tagNames = tags.Select(t => t.Name).ToList();
    return Result<TagsResponse>.Success(new TagsResponse(tagNames));
  }
}
