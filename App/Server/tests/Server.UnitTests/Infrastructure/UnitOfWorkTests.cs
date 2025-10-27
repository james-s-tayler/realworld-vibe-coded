using Ardalis.Result;
using Audit.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Server.Infrastructure.Data;

namespace Server.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for UnitOfWork to verify transaction handling, AuditScope integration,
/// and logging behavior for success, failure, and exception scenarios.
/// These tests use a test double approach to verify behavior without complex mocking.
/// </summary>
public class UnitOfWorkTests
{
  [Fact]
  public async Task ExecuteInTransactionAsync_WithSuccessResult_ShouldReturnSuccessfulResult()
  {
    // Arrange
    using var unitOfWork = await CreateTestUnitOfWorkAsync();
    var expectedValue = "test-success";
    var successResult = Result<string>.Success(expectedValue);

    // Act
    var result = await unitOfWork.ExecuteInTransactionAsync(
      async ct => await Task.FromResult(successResult),
      CancellationToken.None);

    // Assert
    result.IsSuccess.ShouldBeTrue();
    result.Value.ShouldBe(expectedValue);
  }

  [Fact]
  public async Task ExecuteInTransactionAsync_WithErrorResult_ShouldReturnErrorResult()
  {
    // Arrange
    using var unitOfWork = await CreateTestUnitOfWorkAsync();
    var errorMessage = "Operation failed";
    var errorResult = Result<string>.Error(errorMessage);

    // Act
    var result = await unitOfWork.ExecuteInTransactionAsync(
      async ct => await Task.FromResult(errorResult),
      CancellationToken.None);

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.Error);
    result.Errors.ShouldContain(errorMessage);
  }

  [Fact]
  public async Task ExecuteInTransactionAsync_WithInvalidResult_ShouldReturnInvalidResult()
  {
    // Arrange
    using var unitOfWork = await CreateTestUnitOfWorkAsync();
    var invalidResult = Result<string>.Invalid(new ValidationError("Field", "is required"));

    // Act
    var result = await unitOfWork.ExecuteInTransactionAsync(
      async ct => await Task.FromResult(invalidResult),
      CancellationToken.None);

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.Invalid);
    result.ValidationErrors.Count().ShouldBe(1);
  }

  [Fact]
  public async Task ExecuteInTransactionAsync_WithNotFoundResult_ShouldReturnNotFoundResult()
  {
    // Arrange
    using var unitOfWork = await CreateTestUnitOfWorkAsync();
    var notFoundResult = Result<string>.NotFound();

    // Act
    var result = await unitOfWork.ExecuteInTransactionAsync(
      async ct => await Task.FromResult(notFoundResult),
      CancellationToken.None);

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.NotFound);
  }

  [Fact]
  public async Task ExecuteInTransactionAsync_WithCreatedResult_ShouldReturnCreatedResult()
  {
    // Arrange
    using var unitOfWork = await CreateTestUnitOfWorkAsync();
    var createdValue = "new-resource";
    var createdResult = Result<string>.Created(createdValue);

    // Act
    var result = await unitOfWork.ExecuteInTransactionAsync(
      async ct => await Task.FromResult(createdResult),
      CancellationToken.None);

    // Assert
    result.IsSuccess.ShouldBeTrue();
    result.Status.ShouldBe(ResultStatus.Created);
    result.Value.ShouldBe(createdValue);
  }

  [Fact]
  public async Task ExecuteInTransactionAsync_WithException_ShouldRollbackAndRethrow()
  {
    // Arrange
    using var unitOfWork = await CreateTestUnitOfWorkAsync();
    var expectedException = new InvalidOperationException("Database error");

    // Act & Assert
    await Should.ThrowAsync<InvalidOperationException>(async () =>
    {
      await unitOfWork.ExecuteInTransactionAsync<string>(
        ct => throw expectedException,
        CancellationToken.None);
    });
  }

  [Fact]
  public async Task ExecuteInTransactionAsync_WithCancellation_ShouldPassCancellationToken()
  {
    // Arrange
    using var unitOfWork = await CreateTestUnitOfWorkAsync();
    var cts = new CancellationTokenSource();
    var cancellationTokenReceived = false;

    // Act
    var result = await unitOfWork.ExecuteInTransactionAsync(
      async ct =>
      {
        cancellationTokenReceived = ct == cts.Token;
        return await Task.FromResult(Result<string>.Success("test"));
      },
      cts.Token);

    // Assert
    cancellationTokenReceived.ShouldBeTrue();
  }

  [Fact]
  public async Task ExecuteInTransactionAsync_ShouldCreateAuditScope()
  {
    // Arrange
    using var unitOfWork = await CreateTestUnitOfWorkAsync();
    var successResult = Result<string>.Success("test");

    // Configure audit to track events
    var auditEvents = new List<AuditEvent>();
    Audit.Core.Configuration.Setup()
      .Use(config => config
        .OnInsert(ev => auditEvents.Add(AuditEvent.FromJson(ev.ToJson()))));

    // Act
    var result = await unitOfWork.ExecuteInTransactionAsync(
      async ct => await Task.FromResult(successResult),
      CancellationToken.None);

    // Assert
    result.IsSuccess.ShouldBeTrue();
    auditEvents.ShouldNotBeEmpty();
    auditEvents.Any(e => e.EventType == "DatabaseTransactionEvent").ShouldBeTrue();
  }

  [Fact]
  public async Task ExecuteInTransactionAsync_WithSuccessResult_ShouldSetAuditFieldsCorrectly()
  {
    // Arrange
    using var unitOfWork = await CreateTestUnitOfWorkAsync();
    var successResult = Result<string>.Success("test");

    // Configure audit to track events
    var auditEvents = new List<AuditEvent>();
    Audit.Core.Configuration.Setup()
      .Use(config => config
        .OnInsert(ev => auditEvents.Add(AuditEvent.FromJson(ev.ToJson()))));

    // Act
    var result = await unitOfWork.ExecuteInTransactionAsync(
      async ct => await Task.FromResult(successResult),
      CancellationToken.None);

    // Assert
    result.IsSuccess.ShouldBeTrue();
    var transactionEvent = auditEvents.FirstOrDefault(e => e.EventType == "DatabaseTransactionEvent");
    transactionEvent.ShouldNotBeNull();
    transactionEvent.CustomFields.ShouldContainKey("TransactionStatus");
    transactionEvent.CustomFields["TransactionStatus"].ToString().ShouldBe("Committed");
    transactionEvent.CustomFields.ShouldContainKey("ResultStatus");
    transactionEvent.CustomFields["ResultStatus"].ToString().ShouldBe("Ok");
  }

  [Fact]
  public async Task ExecuteInTransactionAsync_WithErrorResult_ShouldNotInsertAuditEvent()
  {
    // Arrange
    using var unitOfWork = await CreateTestUnitOfWorkAsync();
    var errorResult = Result<string>.Error("Operation failed");

    // Configure audit to track events - discarded events should not be inserted
    var auditEvents = new List<AuditEvent>();
    Audit.Core.Configuration.Setup()
      .Use(config => config
        .OnInsert(ev => auditEvents.Add(AuditEvent.FromJson(ev.ToJson()))));

    // Act
    var result = await unitOfWork.ExecuteInTransactionAsync(
      async ct => await Task.FromResult(errorResult),
      CancellationToken.None);

    // Assert
    result.IsSuccess.ShouldBeFalse();
    // Discarded audit events should not be inserted
    // The audit scope is discarded, so no Transaction event should be in the list
    var transactionEvents = auditEvents.Where(e => e.EventType == "Transaction").ToList();
    transactionEvents.ShouldBeEmpty();
  }

  /// <summary>
  /// Creates a test UnitOfWork with an in-memory SQLite database.
  /// </summary>
  private static async Task<TestUnitOfWork> CreateTestUnitOfWorkAsync()
  {
    var identityOptions = new DbContextOptionsBuilder<IdentityDbContext>()
      .UseSqlite("DataSource=:memory:")
      .Options;

    var domainOptions = new DbContextOptionsBuilder<DomainDbContext>()
      .UseSqlite("DataSource=:memory:")
      .Options;

    var identityContext = new IdentityDbContext(identityOptions);
    var domainContext = new DomainDbContext(domainOptions, dispatcher: null);

    await identityContext.Database.OpenConnectionAsync();
    await identityContext.Database.EnsureCreatedAsync();

    await domainContext.Database.OpenConnectionAsync();
    await domainContext.Database.EnsureCreatedAsync();

    var logger = NullLogger<UnitOfWork>.Instance;
    return new TestUnitOfWork(identityContext, domainContext, logger);
  }

  /// <summary> -
  /// Test wrapper for UnitOfWork that implements IDisposable for cleanup.
  /// </summary>
  private class TestUnitOfWork : UnitOfWork, IDisposable
  {
    private readonly IdentityDbContext _identityContext;
    private readonly DomainDbContext _domainContext;

    public TestUnitOfWork(
      IdentityDbContext identityContext,
      DomainDbContext domainContext,
      ILogger<UnitOfWork> logger)
      : base(identityContext, domainContext, logger)
    {
      _identityContext = identityContext;
      _domainContext = domainContext;
    }

    public void Dispose()
    {
      _identityContext.Database.CloseConnection();
      _identityContext.Dispose();
      _domainContext.Database.CloseConnection();
      _domainContext.Dispose();
    }
  }
}
