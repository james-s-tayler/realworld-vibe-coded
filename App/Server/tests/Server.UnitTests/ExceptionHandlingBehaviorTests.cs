using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Server.SharedKernel;

namespace Server.UnitTests;

/// <summary>
/// Unit tests for ExceptionHandlingBehavior to ensure it correctly handles exceptions
/// including DbUpdateConcurrencyException which should be converted to Result.Conflict.
/// </summary>
public class ExceptionHandlingBehaviorTests
{
  private readonly ILogger<ExceptionHandlingBehavior<TestCommand, Result<string>>> _logger;
  private readonly IExceptionResultFactory<Result<string>> _resultFactory;
  private readonly ExceptionHandlingBehavior<TestCommand, Result<string>> _behavior;

  public ExceptionHandlingBehaviorTests()
  {
    _logger = NullLogger<ExceptionHandlingBehavior<TestCommand, Result<string>>>.Instance;
    _resultFactory = new ExceptionResultFactory<Result<string>>();
    _behavior = new ExceptionHandlingBehavior<TestCommand, Result<string>>(_logger, _resultFactory);
  }

  [Fact]
  public async Task Handle_WithSuccess_ShouldReturnSuccessResult()
  {
    // Arrange
    var command = new TestCommand();
    var expectedResult = Result<string>.Success("test");

    // Act
    var result = await _behavior.Handle(command, (ct) => Task.FromResult(expectedResult), CancellationToken.None);

    // Assert
    result.ShouldBe(expectedResult);
    result.IsSuccess.ShouldBeTrue();
  }

  [Fact]
  public async Task Handle_WithDbUpdateConcurrencyException_ShouldReturnConflictResult()
  {
    // Arrange
    var command = new TestCommand();
    var concurrencyException = new DbUpdateConcurrencyException("Concurrency conflict");

    // Act
    var result = await _behavior.Handle(command, (ct) => throw concurrencyException, CancellationToken.None);

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.Conflict);
    result.ValidationErrors.ShouldContain(e => e.ErrorMessage.Contains("Concurrency conflict"));
  }

  [Fact]
  public async Task Handle_WithGenericException_ShouldReturnCriticalErrorResult()
  {
    // Arrange
    var command = new TestCommand();
    var exception = new InvalidOperationException("Something went wrong");

    // Act
    var result = await _behavior.Handle(command, (ct) => throw exception, CancellationToken.None);

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.CriticalError);
    result.ValidationErrors.ShouldContain(e => e.ErrorMessage.Contains("Something went wrong"));
  }

  // Note: With the new factory-based approach, ExceptionHandlingBehavior is designed to work
  // only with Result<T> types through dependency injection. The factory ensures type safety
  // at the DI container level, making the previous test for non-Result types unnecessary.
  // The old test verified runtime behavior when TResponse wasn't Result<T>, but the new
  // design prevents this scenario through compile-time constraints and DI configuration.

  // Test command class
  private record TestCommand : ICommand<Result<string>>;

  // Test command with non-Result response type
  private record TestNonResultCommand : ICommand<string>;
}
