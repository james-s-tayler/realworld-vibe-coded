using Microsoft.AspNetCore.Identity;
using Server.Core.ArticleAggregate;
using Server.Core.CommentAggregate;
using Server.Core.IdentityAggregate;
using Server.Core.UserFollowingAggregate;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;
using Server.UseCases.Articles.Specs;
using Server.UseCases.Profiles;

namespace Server.UseCases.Comments.Create;

public class CreateCommentHandler(
  IReadRepository<Article> articleRepo,
  IRepository<Comment> commentRepo,
  IReadRepository<UserFollowing> followingRepo,
  UserManager<ApplicationUser> userManager)
  : ICommandHandler<CreateCommentCommand, CommentResult>
{
  public async Task<Result<CommentResult>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
  {
    var article = await articleRepo.FirstOrDefaultAsync(
      new ArticleBySlugSpec(request.Slug), cancellationToken);

    if (article == null)
    {
      return Result<CommentResult>.NotFound(request.Slug);
    }

    var author = await userManager.FindByIdAsync(request.AuthorId.ToString());
    if (author == null)
    {
      return Result<CommentResult>.NotFound();
    }

    var comment = new Comment
    {
      Body = request.Body,
      ArticleId = article.Id,
      AuthorId = request.AuthorId,
      Author = author,
    };

    await commentRepo.AddAsync(comment, cancellationToken);

    var authorFollowing = await followingRepo.AnyAsync(
      new UserFollowingByUsersSpec(request.AuthorId, comment.AuthorId), cancellationToken);

    return Result<CommentResult>.Created(new CommentResult(comment, authorFollowing));
  }
}
