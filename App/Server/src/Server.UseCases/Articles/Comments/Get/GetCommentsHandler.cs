using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.ArticleAggregate.Specifications;
using Server.Core.Interfaces;

namespace Server.UseCases.Articles.Comments.Get;

public class GetCommentsHandler(IRepository<Article> _articleRepository)
  : IQueryHandler<GetCommentsQuery, Result<CommentsResponse>>
{
  public async Task<Result<CommentsResponse>> Handle(GetCommentsQuery request, CancellationToken cancellationToken)
  {
    var article = await _articleRepository.FirstOrDefaultAsync(
      new ArticleBySlugSpec(request.Slug), cancellationToken);

    if (article == null)
    {
      return Result.NotFound("Article not found");
    }

    var commentDtos = article.Comments.Select(c => new CommentDto(
      c.Id,
      c.CreatedAt,
      c.UpdatedAt,
      c.Body,
      new AuthorDto(
        c.Author.Username,
        c.Author.Bio ?? string.Empty,
        c.Author.Image,
        request.CurrentUserId.HasValue && request.CurrentUserId.Value != c.AuthorId // Simple following logic for tests
      )
    )).ToList();

    return Result.Success(new CommentsResponse(commentDtos));
  }
}
