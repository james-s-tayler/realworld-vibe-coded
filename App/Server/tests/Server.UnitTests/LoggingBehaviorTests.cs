using Microsoft.Extensions.Logging.Abstractions;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Result;

namespace Server.UnitTests;

public class LoggingBehaviorTests
{
  private record TestQuery(string Value) : IQuery<string>;

  private readonly LoggingBehavior<TestQuery, string> _behavior =
    new(NullLogger<LoggingBehavior<TestQuery, string>>.Instance);

  [Fact]
  public async Task Handle_WithSuccessResult_ReturnsResult()
  {
    var query = new TestQuery("test");
    var expected = Result<string>.Success("result");

    var result = await _behavior.Handle(query, (ct) => Task.FromResult(expected), CancellationToken.None);

    result.ShouldBe(expected);
    result.IsSuccess.ShouldBeTrue();
  }

  [Fact]
  public async Task Handle_WithInvalidResult_ReturnsResultUnchanged()
  {
    var query = new TestQuery("test");
    var expected = Result<string>.Invalid(new ErrorDetail("field", "error"));

    var result = await _behavior.Handle(query, (ct) => Task.FromResult(expected), CancellationToken.None);

    result.ShouldBe(expected);
    result.Status.ShouldBe(ResultStatus.Invalid);
  }

  [Fact]
  public async Task Handle_WithErrorResult_ReturnsResultUnchanged()
  {
    var query = new TestQuery("test");
    var expected = Result<string>.Error(new ErrorDetail("field", "something failed"));

    var result = await _behavior.Handle(query, (ct) => Task.FromResult(expected), CancellationToken.None);

    result.ShouldBe(expected);
    result.Status.ShouldBe(ResultStatus.Error);
  }

  [Fact]
  public async Task Handle_WithNotFoundResult_ReturnsResultUnchanged()
  {
    var query = new TestQuery("test");
    var expected = Result<string>.NotFound();

    var result = await _behavior.Handle(query, (ct) => Task.FromResult(expected), CancellationToken.None);

    result.ShouldBe(expected);
    result.Status.ShouldBe(ResultStatus.NotFound);
  }

  [Fact]
  public async Task Handle_WithForbiddenResult_ReturnsResultUnchanged()
  {
    var query = new TestQuery("test");
    var expected = Result<string>.Forbidden();

    var result = await _behavior.Handle(query, (ct) => Task.FromResult(expected), CancellationToken.None);

    result.ShouldBe(expected);
    result.Status.ShouldBe(ResultStatus.Forbidden);
  }

  [Fact]
  public async Task Handle_WithConflictResult_ReturnsResultUnchanged()
  {
    var query = new TestQuery("test");
    var expected = Result<string>.Conflict();

    var result = await _behavior.Handle(query, (ct) => Task.FromResult(expected), CancellationToken.None);

    result.ShouldBe(expected);
    result.Status.ShouldBe(ResultStatus.Conflict);
  }

  [Fact]
  public async Task Handle_WithCriticalErrorResult_ReturnsResultUnchanged()
  {
    var query = new TestQuery("test");
    var expected = Result<string>.CriticalError(new ErrorDetail("field", "critical failure"));

    var result = await _behavior.Handle(query, (ct) => Task.FromResult(expected), CancellationToken.None);

    result.ShouldBe(expected);
    result.Status.ShouldBe(ResultStatus.CriticalError);
  }
}
