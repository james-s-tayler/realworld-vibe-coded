using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Server.Infrastructure.Data;
using Server.SharedKernel;

namespace Server.UnitTests;

/// <summary>
/// Unit tests for TransactionBehavior to ensure it correctly wraps Commands in transactions
/// and only commits when Result.IsSuccess is true.
/// </summary>
public class TransactionBehaviorTests
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly ILogger<TransactionBehavior<TestCommand, Result<string>>> _logger;
  private readonly TransactionBehavior<TestCommand, Result<string>> _behavior;

  public TransactionBehaviorTests()
  {
    _unitOfWork = Substitute.For<IUnitOfWork>();
    _logger = NullLogger<TransactionBehavior<TestCommand, Result<string>>>.Instance;
    _behavior = new TransactionBehavior<TestCommand, Result<string>>(_unitOfWork, _logger);
  }

  [Fact]
  public async Task Handle_WithCommand_ShouldExecuteInTransaction()
  {
    // Arrange
    var command = new TestCommand();
    var expectedResult = Result<string>.Success("test");

    _unitOfWork.ExecuteInTransactionAsync(
      Arg.Any<Func<CancellationToken, Task<Result<string>>>>(),
      Arg.Any<CancellationToken>())
      .Returns(callInfo =>
      {
        var operation = callInfo.Arg<Func<CancellationToken, Task<Result<string>>>>();
        return operation(CancellationToken.None);
      });

    // Act
    var result = await _behavior.Handle(command, (ct) => Task.FromResult(expectedResult), CancellationToken.None);

    // Assert
    result.ShouldBe(expectedResult);
    await _unitOfWork.Received(1).ExecuteInTransactionAsync(
      Arg.Any<Func<CancellationToken, Task<Result<string>>>>(),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_WithQuery_ShouldNotExecuteInTransaction()
  {
    // Arrange
    var query = new TestQuery();
    var expectedResult = Result<string>.Success("test");
    var queryBehavior = new TransactionBehavior<TestQuery, Result<string>>(_unitOfWork,
      NullLogger<TransactionBehavior<TestQuery, Result<string>>>.Instance);

    // Act
    var result = await queryBehavior.Handle(query, (ct) => Task.FromResult(expectedResult), CancellationToken.None);

    // Assert
    result.ShouldBe(expectedResult);
    await _unitOfWork.DidNotReceive().ExecuteInTransactionAsync(
      Arg.Any<Func<CancellationToken, Task<Result<string>>>>(),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_WithSuccessResult_ShouldCommitTransaction()
  {
    // Arrange
    var command = new TestCommand();
    var successResult = Result<string>.Success("test");
    var executedInTransaction = false;

    _unitOfWork.ExecuteInTransactionAsync(
      Arg.Any<Func<CancellationToken, Task<Result<string>>>>(),
      Arg.Any<CancellationToken>())
      .Returns(callInfo =>
      {
        executedInTransaction = true;
        var operation = callInfo.Arg<Func<CancellationToken, Task<Result<string>>>>();
        return operation(CancellationToken.None);
      });

    // Act
    var result = await _behavior.Handle(command, (ct) => Task.FromResult(successResult), CancellationToken.None);

    // Assert
    result.IsSuccess.ShouldBeTrue();
    executedInTransaction.ShouldBeTrue();
  }

  [Fact]
  public async Task Handle_WithErrorResult_ShouldRollbackTransaction()
  {
    // Arrange
    var command = new TestCommand();
    var errorResult = Result<string>.Error("Something went wrong");
    var executedInTransaction = false;

    _unitOfWork.ExecuteInTransactionAsync(
      Arg.Any<Func<CancellationToken, Task<Result<string>>>>(),
      Arg.Any<CancellationToken>())
      .Returns(callInfo =>
      {
        executedInTransaction = true;
        var operation = callInfo.Arg<Func<CancellationToken, Task<Result<string>>>>();
        return operation(CancellationToken.None);
      });

    // Act
    var result = await _behavior.Handle(command, (ct) => Task.FromResult(errorResult), CancellationToken.None);

    // Assert
    result.IsSuccess.ShouldBeFalse();
    executedInTransaction.ShouldBeTrue();
  }

  [Fact]
  public async Task Handle_WithCriticalErrorResult_ShouldRollbackTransaction()
  {
    // Arrange
    var command = new TestCommand();
    var criticalErrorResult = CustomArdalisResultFactory.CriticalError<string>(
      new ValidationError("TestError", "Critical error occurred"));
    var executedInTransaction = false;

    _unitOfWork.ExecuteInTransactionAsync(
      Arg.Any<Func<CancellationToken, Task<Result<string>>>>(),
      Arg.Any<CancellationToken>())
      .Returns(callInfo =>
      {
        executedInTransaction = true;
        var operation = callInfo.Arg<Func<CancellationToken, Task<Result<string>>>>();
        return operation(CancellationToken.None);
      });

    // Act
    var result = await _behavior.Handle(command, (ct) => Task.FromResult(criticalErrorResult), CancellationToken.None);

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.CriticalError);
    executedInTransaction.ShouldBeTrue();
  }

  [Fact]
  public async Task Handle_WithInvalidResult_ShouldRollbackTransaction()
  {
    // Arrange
    var command = new TestCommand();
    var invalidResult = Result<string>.Invalid(new ValidationError("Field", "is invalid"));
    var executedInTransaction = false;

    _unitOfWork.ExecuteInTransactionAsync(
      Arg.Any<Func<CancellationToken, Task<Result<string>>>>(),
      Arg.Any<CancellationToken>())
      .Returns(callInfo =>
      {
        executedInTransaction = true;
        var operation = callInfo.Arg<Func<CancellationToken, Task<Result<string>>>>();
        return operation(CancellationToken.None);
      });

    // Act
    var result = await _behavior.Handle(command, (ct) => Task.FromResult(invalidResult), CancellationToken.None);

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.Invalid);
    executedInTransaction.ShouldBeTrue();
  }

  [Fact]
  public async Task Handle_WithCreatedResult_ShouldCommitTransaction()
  {
    // Arrange
    var command = new TestCommand();
    var createdResult = Result<string>.Created("new-resource");
    var executedInTransaction = false;

    _unitOfWork.ExecuteInTransactionAsync(
      Arg.Any<Func<CancellationToken, Task<Result<string>>>>(),
      Arg.Any<CancellationToken>())
      .Returns(callInfo =>
      {
        executedInTransaction = true;
        var operation = callInfo.Arg<Func<CancellationToken, Task<Result<string>>>>();
        return operation(CancellationToken.None);
      });

    // Act
    var result = await _behavior.Handle(command, (ct) => Task.FromResult(createdResult), CancellationToken.None);

    // Assert
    result.IsSuccess.ShouldBeTrue();
    result.Status.ShouldBe(ResultStatus.Created);
    executedInTransaction.ShouldBeTrue();
  }

  [Fact]
  public async Task Handle_WithDbUpdateConcurrencyException_ShouldReturnConflictResult()
  {
    // Arrange
    var command = new TestCommand();
    var concurrencyException = new DbUpdateConcurrencyException("Concurrency conflict");

    _unitOfWork.ExecuteInTransactionAsync(
      Arg.Any<Func<CancellationToken, Task<Result<string>>>>(),
      Arg.Any<CancellationToken>())
      .Returns<Task<Result<string>>>(callInfo => throw concurrencyException);

    // Act
    var result = await _behavior.Handle(command, (ct) => Task.FromResult(Result<string>.Success("test")), CancellationToken.None);

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.Conflict);
    result.Errors.ShouldContain(e => e.Contains("concurrency conflict"));
  }

  // Test command and query classes
  private record TestCommand : ICommand<Result<string>>;
  private record TestQuery : IQuery<Result<string>>;
}

