namespace Server.Web.DevOnly.Endpoints.WipeData;

/// <summary>
/// Endpoint to wipe all users and user-generated content from the database.
/// </summary>
/// <remarks>
/// This endpoint is used for E2E test cleanup to ensure test isolation.
/// It deletes all users, articles, comments, tags, and user followings.
/// </remarks>
public class WipeData(IMediator mediator) : Endpoint<EmptyRequest>
{
  public override void Configure()
  {
    Delete("wipe");
    Group<TestData>();
    Summary(s =>
    {
      s.Summary = "Wipe all user-generated content";
      s.Description = "Deletes all users, articles, comments, tags, and user followings from the database. Used for E2E test cleanup.";
    });
  }

  public override async Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
  {
    var result = await mediator.Send(new WipeDataCommand(), cancellationToken);
    await Send.ResultValueAsync(result, cancellationToken);
  }
}
