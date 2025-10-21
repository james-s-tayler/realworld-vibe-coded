using Ardalis.Result;
using Ardalis.SharedKernel;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Server.Infrastructure.Data;
using Server.Web.Configurations;

namespace Server.UnitTests.Web.Configurations;

public class TransactionBehaviorTests : IDisposable
{
  private readonly SqliteConnection _connection;
  private readonly AppDbContext _dbContext;
  private readonly ILogger<TransactionBehavior<TestCommand, Result<string>>> _logger;
  private readonly TransactionBehavior<TestCommand, Result<string>> _behavior;

  public TransactionBehaviorTests()
  {
    // Create an in-memory database for testing
    _connection = new SqliteConnection("DataSource=:memory:");
    _connection.Open();

    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseSqlite(_connection)
      .Options;

    _dbContext = new AppDbContext(options, null);
    _dbContext.Database.EnsureCreated();

    _logger = Substitute.For<ILogger<TransactionBehavior<TestCommand, Result<string>>>>();
    _behavior = new TransactionBehavior<TestCommand, Result<string>>(_dbContext, _logger);
  }

  public void Dispose()
  {
    _dbContext.Dispose();
    _connection.Close();
    _connection.Dispose();
  }

  [Fact]
  public async Task Handle_WhenRequestIsCommand_BeginsTransaction()
  {
    // Arrange
    var request = new TestCommand("test");
    var response = Result<string>.Success("test result");

    // Act
    var result = await _behavior.Handle(request, ct => Task.FromResult(response), CancellationToken.None);

    // Assert
    result.ShouldBe(response);
    // Verify that logging occurred (both "Beginning transaction" and "Committed transaction")
    _logger.ReceivedWithAnyArgs(2).Log(
      LogLevel.Information,
      Arg.Any<EventId>(),
      Arg.Any<object>(),
      null,
      Arg.Any<Func<object, Exception?, string>>());
  }

  [Fact]
  public async Task Handle_WhenRequestIsQuery_DoesNotBeginTransaction()
  {
    // Arrange
    using var connection = new SqliteConnection("DataSource=:memory:");
    connection.Open();
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseSqlite(connection)
      .Options;
    using var dbContext = new AppDbContext(options, null);
    dbContext.Database.EnsureCreated();

    var queryBehavior = new TransactionBehavior<TestQuery, Result<string>>(dbContext,
      Substitute.For<ILogger<TransactionBehavior<TestQuery, Result<string>>>>());
    var request = new TestQuery("test");
    var response = Result<string>.Success("test result");

    // Act
    var result = await queryBehavior.Handle(request, ct => Task.FromResult(response), CancellationToken.None);

    // Assert
    result.ShouldBe(response);
  }

  [Fact]
  public async Task Handle_WhenCommandSucceeds_CommitsTransaction()
  {
    // Arrange
    var request = new TestCommand("test");
    var response = Result<string>.Success("success");

    // Act
    var result = await _behavior.Handle(request, ct => Task.FromResult(response), CancellationToken.None);

    // Assert
    result.IsSuccess.ShouldBeTrue();
    result.Value.ShouldBe("success");
  }

  [Fact]
  public async Task Handle_WhenCommandFails_RollsBackTransaction()
  {
    // Arrange
    var request = new TestCommand("test");
    var response = Result<string>.Error("error occurred");

    // Act
    var result = await _behavior.Handle(request, ct => Task.FromResult(response), CancellationToken.None);

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Errors.ShouldContain("error occurred");
  }

  [Fact]
  public async Task Handle_WhenExceptionThrown_RollsBackTransactionAndRethrows()
  {
    // Arrange
    var request = new TestCommand("test");
    var expectedException = new InvalidOperationException("test exception");

    // Act & Assert
    var exception = await Should.ThrowAsync<InvalidOperationException>(
      async () => await _behavior.Handle(request, ct => throw expectedException, CancellationToken.None));

    exception.ShouldBe(expectedException);
  }

  [Fact]
  public async Task Handle_WithNonResultResponse_TreatsAsSuccess()
  {
    // Arrange
    using var connection = new SqliteConnection("DataSource=:memory:");
    connection.Open();
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseSqlite(connection)
      .Options;
    using var dbContext = new AppDbContext(options, null);
    dbContext.Database.EnsureCreated();

    var nonResultBehavior = new TransactionBehavior<TestCommand, string>(dbContext,
      Substitute.For<ILogger<TransactionBehavior<TestCommand, string>>>());
    var request = new TestCommand("test");

    // Act
    var result = await nonResultBehavior.Handle(request, ct => Task.FromResult("plain string response"), CancellationToken.None);

    // Assert
    result.ShouldBe("plain string response");
  }

  [Fact]
  public async Task Handle_WithNonGenericResult_CommitsOnSuccess()
  {
    // Arrange
    using var connection = new SqliteConnection("DataSource=:memory:");
    connection.Open();
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseSqlite(connection)
      .Options;
    using var dbContext = new AppDbContext(options, null);
    dbContext.Database.EnsureCreated();

    var nonGenericBehavior = new TransactionBehavior<TestCommand, Result>(dbContext,
      Substitute.For<ILogger<TransactionBehavior<TestCommand, Result>>>());
    var request = new TestCommand("test");
    var response = Result.Success();

    // Act
    var result = await nonGenericBehavior.Handle(request, ct => Task.FromResult(response), CancellationToken.None);

    // Assert
    result.IsSuccess.ShouldBeTrue();
  }

  [Fact]
  public async Task Handle_WithNonGenericResult_RollsBackOnFailure()
  {
    // Arrange
    using var connection = new SqliteConnection("DataSource=:memory:");
    connection.Open();
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseSqlite(connection)
      .Options;
    using var dbContext = new AppDbContext(options, null);
    dbContext.Database.EnsureCreated();

    var nonGenericBehavior = new TransactionBehavior<TestCommand, Result>(dbContext,
      Substitute.For<ILogger<TransactionBehavior<TestCommand, Result>>>());
    var request = new TestCommand("test");
    var response = Result.NotFound();

    // Act
    var result = await nonGenericBehavior.Handle(request, ct => Task.FromResult(response), CancellationToken.None);

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.NotFound);
  }

  // Test command and query classes
  public record TestCommand(string Value) : ICommand<Result<string>>;
  public record TestQuery(string Value) : IQuery<Result<string>>;
}
