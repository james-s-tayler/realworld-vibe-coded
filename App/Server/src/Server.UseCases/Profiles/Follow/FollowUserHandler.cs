using Server.Core.AuthorAggregate;
using Server.Core.AuthorAggregate.Specifications;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Profiles.Follow;

public class FollowUserHandler(IRepository<Author> authorRepository)
  : ICommandHandler<FollowUserCommand, Author>
{
  public async Task<Result<Author>> Handle(FollowUserCommand request, CancellationToken cancellationToken)
  {
    // Find the author to follow
    var authorToFollow = await authorRepository.FirstOrDefaultAsync(
      new AuthorByUsernameSpec(request.Username), cancellationToken);

    if (authorToFollow == null)
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

    // Follow the author
    currentAuthor.Follow(authorToFollow);
    await authorRepository.UpdateAsync(currentAuthor, cancellationToken);

    return Result<Author>.Success(authorToFollow);
  }
}
