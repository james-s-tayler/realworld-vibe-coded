namespace Server.Web.DevOnly.Endpoints;

/// <summary>
/// Endpoint that clears all test data from the database.
/// Used by E2E tests to ensure a clean state between tests.
/// </summary>
public class ResetDatabase(IMediator mediator) : Endpoint<EmptyRequest>
{
  public override void Configure()
  {
    Delete("reset");
    Group<TestData>();
    Summary(s =>
    {
      s.Summary = "Reset database - clear all test data";
      s.Description = "Clears all data from the database. Used by E2E tests for cleanup.";
    });
  }

  public override async Task HandleAsync(EmptyRequest request, CancellationToken cancellationToken)
  {
    var result = await mediator.Send(new ResetDatabaseCommand(), cancellationToken);

    await Send.ResultValueAsync(result, cancellationToken);
  }
}
