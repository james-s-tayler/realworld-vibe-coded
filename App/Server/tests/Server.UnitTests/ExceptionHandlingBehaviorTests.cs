using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Server.SharedKernel.MediatR;
using Server.SharedKernel.Result;

namespace Server.UnitTests;

/// <summary>
/// Unit tests for ExceptionHandlingBehavior to ensure it correctly handles exceptions
/// including DbUpdateConcurrencyException which should be converted to Result.Conflict.
/// </summary>
public class ExceptionHandlingBehaviorTests
{
  private readonly ILogger<ExceptionHandlingBehavior<TestCommand, string>> _logger;
  private readonly ExceptionHandlingBehavior<TestCommand, string> _behavior;

  public ExceptionHandlingBehaviorTests()
  {
    _logger = NullLogger<ExceptionHandlingBehavior<TestCommand, string>>.Instance;
    _behavior = new ExceptionHandlingBehavior<TestCommand, string>(_logger);
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
    result.ErrorDetails.ShouldContain(e => e.ErrorMessage.Contains("Concurrency conflict"));
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
    result.ErrorDetails.ShouldContain(e => e.ErrorMessage.Contains("Something went wrong"));
  }

  [Fact]
  public async Task Handle_WithDbUpdateExceptionContainingDuplicateKeyMessage_ShouldReturnConflictResult()
  {
    var command = new TestCommand();
    var innerException = new InvalidOperationException("Cannot insert duplicate key row in object");
    var dbUpdateException = new DbUpdateException("An error occurred", innerException);

    var result = await _behavior.Handle(command, (ct) => throw dbUpdateException, CancellationToken.None);

    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.Conflict);
  }

  [Fact]
  public async Task Handle_WithDbUpdateExceptionWithoutDuplicateKey_ShouldReturnCriticalError()
  {
    var command = new TestCommand();
    var dbUpdateException = new DbUpdateException("Some other database error");

    var result = await _behavior.Handle(command, (ct) => throw dbUpdateException, CancellationToken.None);

    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.CriticalError);
  }

  [Fact]
  public async Task Handle_WithNestedDuplicateKeyMessage_ShouldReturnConflictResult()
  {
    var command = new TestCommand();
    var innerMost = new Exception("Violation of UNIQUE KEY constraint. Cannot insert duplicate key in object.");
    var inner = new InvalidOperationException("Database error", innerMost);
    var outer = new DbUpdateException("Save failed", inner);

    var result = await _behavior.Handle(command, (ct) => throw outer, CancellationToken.None);

    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.Conflict);
  }

  // Test command class
  private record TestCommand : ICommand<string>;
}
