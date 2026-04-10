using Microsoft.Extensions.Localization;
using Server.Core.AuthorAggregate;
using Server.Core.AuthorAggregate.Specifications;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;
using Server.SharedKernel.Resources;

namespace Server.UseCases.Profiles.Unfollow;

public class UnfollowUserHandler(IRepository<Author> authorRepository, IStringLocalizer localizer)
  : ICommandHandler<UnfollowUserCommand, Author>
{
  public async Task<Result<Author>> Handle(UnfollowUserCommand request, CancellationToken cancellationToken)
  {
    // Find the author to unfollow
    var authorToUnfollow = await authorRepository.FirstOrDefaultAsync(
      new AuthorByUsernameSpec(request.Username), cancellationToken);

    if (authorToUnfollow == null)
    {
      return Result<Author>.NotFound(request.Username);
    }

    // Get current author with following relationships
    var currentAuthor = await authorRepository.FirstOrDefaultAsync(
      new AuthorWithFollowingByUserIdSpec(request.CurrentUserId), cancellationToken);

    if (currentAuthor == null)
    {
      return Result<Author>.ErrorMissingRequiredEntity(typeof(Author), request.CurrentUserId);
    }

    // Check if the author is currently following the target author
    if (!currentAuthor.IsFollowing(authorToUnfollow))
    {
      return Result<Author>.Invalid(new ErrorDetail("username", localizer[SharedResource.Keys.NotBeingFollowed]));
    }

    // Unfollow the author
    currentAuthor.Unfollow(authorToUnfollow);
    await authorRepository.UpdateAsync(currentAuthor, cancellationToken);

    return Result<Author>.Success(authorToUnfollow);
  }
}
