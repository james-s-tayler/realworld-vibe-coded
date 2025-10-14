using Server.UseCases.Contributors.Get;
using Server.Web.Infrastructure;

namespace Server.Web.Contributors;

/// <summary>
/// Get a Contributor by integer ID.
/// </summary>
/// <remarks>
/// Takes a positive integer ID and returns a matching Contributor record.
/// </remarks>
public class GetById(IMediator _mediator)
  : Endpoint<GetContributorByIdRequest, ContributorRecord>
{
  public override void Configure()
  {
    Get(GetContributorByIdRequest.Route);
    AllowAnonymous();
  }

  public override async Task HandleAsync(GetContributorByIdRequest request,
    CancellationToken cancellationToken)
  {
    var query = new GetContributorQuery(request.ContributorId);

    var result = await _mediator.Send(query, cancellationToken);

    await this.SendAsync(result, contributor => new ContributorRecord(contributor.Id, contributor.Name, contributor.PhoneNumber), cancellationToken);
  }
}
