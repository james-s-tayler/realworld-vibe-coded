using Server.Core.AuthorAggregate;
using Server.Core.AuthorAggregate.Specifications;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Persistence;

namespace Server.UseCases.Profiles.Get;

public class GetProfileHandler(IRepository<Author> authorRepository)
  : IQueryHandler<GetProfileQuery, Author>
{
  public async Task<Result<Author>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
  {
    var author = await authorRepository.FirstOrDefaultAsync(
      new AuthorWithRelationshipsByUsernameSpec(request.Username), cancellationToken);

    if (author == null)
    {
      return Result<Author>.NotFound(request.Username);
    }

    return Result<Author>.Success(author);
  }
}
