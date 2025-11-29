using Server.SharedKernel.MediatR;

namespace Server.Web.DevOnly.UseCases;

#pragma warning disable SRV015 // DevOnly test endpoint
public class ThrowNestedExceptionFromUseCaseHandler : IQueryHandler<ThrowNestedExceptionFromUseCaseQuery, Unit>
{
  /// <summary>
  /// Innermost exception message.
  /// </summary>
  public const string InnerMostExceptionMessage = "This is the innermost exception from use case";

  /// <summary>
  /// Middle exception message.
  /// </summary>
  public const string MiddleExceptionMessage = "This is the middle exception from use case";

  /// <summary>
  /// Outermost exception message.
  /// </summary>
  public const string OuterExceptionMessage = "This is the outer exception from use case";

  public Task<Result<Unit>> Handle(ThrowNestedExceptionFromUseCaseQuery request, CancellationToken cancellationToken)
  {
    var innerMostException = new InvalidOperationException(InnerMostExceptionMessage);
    var middleException = new ApplicationException(MiddleExceptionMessage, innerMostException);
    var outerException = new Exception(OuterExceptionMessage, middleException);

    throw outerException;
  }
}
#pragma warning restore SRV015
