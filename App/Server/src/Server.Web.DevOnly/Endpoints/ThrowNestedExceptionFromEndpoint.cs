namespace Server.Web.DevOnly.Endpoints;

/// <summary>
/// Test endpoint that throws nested exceptions directly in the endpoint to test unwrapping inner exceptions.
/// </summary>
/// <remarks>
/// This endpoint is used to test that the global exception handler correctly
/// unwraps and includes all inner exception messages in the error response.
/// </remarks>
public class ThrowNestedExceptionFromEndpoint : Endpoint<EmptyRequest>
{
  /// <summary>
  /// Innermost exception message.
  /// </summary>
  public const string InnerMostExceptionMessage = "This is the innermost exception from endpoint";

  /// <summary>
  /// Middle exception message.
  /// </summary>
  public const string MiddleExceptionMessage = "This is the middle exception from endpoint";

  /// <summary>
  /// Outermost exception message.
  /// </summary>
  public const string OuterExceptionMessage = "This is the outer exception from endpoint";

  public override void Configure()
  {
    Get("throw-nested-exception-from-endpoint");
    Group<TestError>();
    Summary(s =>
    {
      s.Summary = "Test endpoint - throws nested exceptions directly in endpoint";
      s.Description = "This endpoint throws a three-level nested exception directly in the endpoint to test that the global exception handler correctly unwraps inner exceptions.";
    });
  }

  public override Task HandleAsync(EmptyRequest req, CancellationToken cancellationToken)
  {
    var innerMostException = new InvalidOperationException(InnerMostExceptionMessage);
    var middleException = new ApplicationException(MiddleExceptionMessage, innerMostException);
    var outerException = new Exception(OuterExceptionMessage, middleException);

    throw outerException;
  }
}
