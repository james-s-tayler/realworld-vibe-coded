using Server.UseCases.Contributors.Get;
using Server.UseCases.Contributors.Update;
using Server.Web.Infrastructure;

namespace Server.Web.Contributors;

/// <summary>
/// Update an existing Contributor.
/// </summary>
/// <remarks>
/// Update an existing Contributor by providing a fully defined replacement set of values.
/// See: https://stackoverflow.com/questions/60761955/rest-update-best-practice-put-collection-id-without-id-in-body-vs-put-collecti
/// </remarks>
public class Update(IMediator _mediator)
  : Endpoint<UpdateContributorRequest, UpdateContributorResponse>
{
  public override void Configure()
  {
    Put(UpdateContributorRequest.Route);
    AllowAnonymous();
  }

  public override async Task HandleAsync(
    UpdateContributorRequest request,
    CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new UpdateContributorCommand(request.Id, request.Name!), cancellationToken);

    if (!result.IsSuccess)
    {
      await this.SendAsync(result, cancellationToken);
      return;
    }

    var query = new GetContributorQuery(request.ContributorId);
    var queryResult = await _mediator.Send(query, cancellationToken);

    await this.SendAsync(queryResult, dto => new UpdateContributorResponse(new ContributorRecord(dto.Id, dto.Name, dto.PhoneNumber)), cancellationToken);
  }
}
