using Server.Core.Interfaces;
using Server.SharedKernel.MediatR;

namespace Server.UseCases.Tags.List;

public class ListTagsHandler(IListTagsQueryService _query)
  : IQueryHandler<ListTagsQuery, TagsResponse>
{
  public async Task<Result<TagsResponse>> Handle(ListTagsQuery request, CancellationToken cancellationToken)
  {
    var tags = await _query.ListAsync();
    return Result<TagsResponse>.Success(new TagsResponse(tags.ToList()));
  }
}
