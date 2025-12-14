using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.ArticleAggregate.Specifications.Articles;
using Server.Core.IdentityAggregate;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Articles.Comments.Get;

public class GetCommentsHandler(IRepository<Article> articleRepository, UserManager<ApplicationUser> userManager)
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

    // Get current user with following relationships if authenticated
    ApplicationUser? currentUser = null;
    if (request.CurrentUserId.HasValue)
    {
      currentUser = await userManager.Users
        .Include(u => u.Following)
        .FirstOrDefaultAsync(u => u.Id == request.CurrentUserId.Value, cancellationToken);
    }

    var commentDtos = article.Comments.Select(c => CommentMappers.MapToDto(c, currentUser)).ToList();

    return Result<CommentsResponse>.Success(new CommentsResponse(commentDtos));
  }
}
