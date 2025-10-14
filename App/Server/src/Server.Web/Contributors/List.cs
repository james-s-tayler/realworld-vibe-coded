using Server.UseCases.Contributors;
using Server.UseCases.Contributors.List;
using Server.Web.Infrastructure;

namespace Server.Web.Contributors;

/// <summary>
/// List all Contributors
/// </summary>
/// <remarks>
/// List all contributors - returns a ContributorListResponse containing the Contributors.
/// </remarks>
public class List(IMediator _mediator) : EndpointWithoutRequest<ContributorListResponse>
{
  public override void Configure()
  {
    Get("/api/contributors");
    AllowAnonymous();
  }

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    Result<IEnumerable<ContributorDTO>> result = await _mediator.Send(new ListContributorsQuery(null, null), cancellationToken);

    var result2 = await new ListContributorsQuery2(null, null)
      .ExecuteAsync(cancellationToken);

    await this.SendAsync(result, contributors => new ContributorListResponse
    {
      Contributors = contributors.Select(c => new ContributorRecord(c.Id, c.Name, c.PhoneNumber)).ToList()
    }, cancellationToken);
  }
}
