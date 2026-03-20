using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.ArticleAggregate.Specifications.Articles;
using Server.Core.AuthorAggregate;
using Server.Core.AuthorAggregate.Specifications;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Articles.Comments.Get;

public class GetCommentsHandler(IRepository<Article> articleRepository, IReadRepository<Author> authorRepository)
  : IQueryHandler<GetCommentsQuery, CommentsResponse>
{
  public async Task<Result<CommentsResponse>> Handle(GetCommentsQuery request, CancellationToken cancellationToken)
  {
    var article = await articleRepository.FirstOrDefaultAsync(
      new ArticleBySlugSpec(request.Slug), cancellationToken);

    if (article == null)
    {
      return Result<CommentsResponse>.ErrorMissingRequiredEntity(typeof(Article), request.Slug);
    }

    // Get current author's following relationships if authenticated
    ICollection<AuthorFollowing> currentFollowing = [];
    if (request.CurrentUserId.HasValue)
    {
      var currentAuthor = await authorRepository.FirstOrDefaultAsync(
        new AuthorWithFollowingByUserIdSpec(request.CurrentUserId.Value), cancellationToken);
      currentFollowing = currentAuthor?.Following ?? [];
    }

    var commentDtos = article.Comments.Select(c =>
      CommentMappers.MapToDto(c, currentFollowing.Any(f => f.FollowedId == c.AuthorId))).ToList();

    return Result<CommentsResponse>.Success(new CommentsResponse(commentDtos));
  }
}
