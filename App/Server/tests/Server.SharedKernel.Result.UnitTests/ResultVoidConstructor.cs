using Ardalis.Result;
using FluentAssertions;
using Xunit;
using Result = Ardalis.Result.Result;

namespace Server.SharedKernel.ResultUnitTests;

public class ResultVoidConstructor
{
  [Fact]
  public void InitializesSuccessResultWithConstructor()
  {
    var result = new Result();

    result.Value.Should().BeNull();
    result.Status.Should().Be(ResultStatus.Ok);
  }

  [Fact]
  public void InitializesSuccessResultWithFactoryMethod()
  {
    var result = Result.Success();

    result.Value.Should().BeNull();
    result.Status.Should().Be(ResultStatus.Ok);
  }

  [Fact]
  public void InitializesSuccessResultWithMessageWithFactoryMethod()
  {
    var message = "success";
    var result = Result.SuccessWithMessage(message);

    result.Value.Should().BeNull();
    result.Status.Should().Be(ResultStatus.Ok);
    result.SuccessMessage.Should().Be(message);
  }

  [Theory]
  [InlineData("test1")]
  [InlineData("test1", "test2")]
  public void InitializesErrorResultWithFactoryMethod(params string[] errors)
  {
    var result = Result.Error(new ErrorList(errors, null));

    result.Value.Should().BeNull();
    result.Status.Should().Be(ResultStatus.Error);

    if (errors == null)
    {
      return;
    }

    foreach (var error in errors)
    {
      result.Errors.Should().ContainEquivalentOf(error);
    }
  }

  [Fact]
  public void InitializesErrorResultWithCorrelationIdWithFactoryMethod()
  {
    var correlationId = "testId";
    var errors = new string[] { "Error 1", "Error 2" };
    var result = Result.Error(new ErrorList(errors, correlationId));

    result.Value.Should().BeNull();
    result.Status.Should().Be(ResultStatus.Error);
    result.CorrelationId.Should().Be(correlationId);

    foreach (var error in errors)
    {
      result.Errors.Should().ContainEquivalentOf(error);
    }
  }

  [Fact]
  public void InitializesInvalidResultWithMultipleValidationErrorsWithFactoryMethod()
  {
    var validationErrors = new List<ValidationError>
            {
                new ValidationError
                {
                    Identifier = "name",
                    ErrorMessage = "Name is required"
                },
                new ValidationError
                {
                    Identifier = "postalCode",
                    ErrorMessage = "PostalCode cannot exceed 10 characters"
                }
            };

    var result = Result.Invalid(validationErrors);

    result.Value.Should().BeNull();
    result.Status.Should().Be(ResultStatus.Invalid);

    result.ValidationErrors.Should().ContainEquivalentOf(new ValidationError { ErrorMessage = "Name is required", Identifier = "name" });
    result.ValidationErrors.Should().ContainEquivalentOf(new ValidationError { ErrorMessage = "PostalCode cannot exceed 10 characters", Identifier = "postalCode" });
  }

  [Fact]
  public void InitializesInvalidResultWithSingleValidationErrorWithFactoryMethod()
  {
    var validationError = new ValidationError
    {
      Identifier = "name",
      ErrorMessage = "Name is required"
    };

    var result = Result.Invalid(validationError);

    result.Value.Should().BeNull();
    result.Status.Should().Be(ResultStatus.Invalid);

    result.ValidationErrors.Should().ContainEquivalentOf(new ValidationError { ErrorMessage = "Name is required", Identifier = "name" });
  }

  [Fact]
  public void InitializesNotFoundResultWithFactoryMethod()
  {
    var result = Result.NotFound();

    result.Value.Should().BeNull();
    result.Status.Should().Be(ResultStatus.NotFound);
  }

  [Fact]
  public void InitializesNotFoundResultWithFactoryMethodWithErrors()
  {
    var errorMessage = "User Not Found";
    var result = Result.NotFound(errorMessage);

    result.Value.Should().BeNull();
    result.Status.Should().Be(ResultStatus.NotFound);
    result.Errors.Single().Should().Be(errorMessage);
  }

  [Fact]
  public void InitializesForbiddenResultWithFactoryMethod()
  {
    var result = Result.Forbidden();

    result.Value.Should().BeNull();
    result.Status.Should().Be(ResultStatus.Forbidden);
  }

  [Fact]
  public void InitializesStatusToForbiddenGivenForbiddenFactoryCallWithString()
  {
    var errorMessage = "You are forbidden";
    var result = Result<object>.Forbidden(errorMessage);

    result.Value.Should().BeNull();
    result.Status.Should().Be(ResultStatus.Forbidden);
    result.Errors.Single().Should().Be(errorMessage);
  }

  [Fact]
  public void InitializesUnauthorizedResultWithFactoryMethod()
  {
    var result = Result.Unauthorized();

    result.Value.Should().BeNull();
    result.Status.Should().Be(ResultStatus.Unauthorized);
  }

  [Fact]
  public void InitializesUnauthorizedResultWithFactoryMethodWithString()
  {
    var errorMessage = "You are unauthorized";
    var result = Result.Unauthorized(errorMessage);

    result.Value.Should().BeNull();
    result.Status.Should().Be(ResultStatus.Unauthorized);
    result.Errors.Single().Should().Be(errorMessage);
  }

  [Fact]
  public void InitializesConflictResultWithFactoryMethod()
  {
    var result = Result.Conflict();

    result.Value.Should().BeNull();
    result.Status.Should().Be(ResultStatus.Conflict);
  }

  [Fact]
  public void InitializesConflictResultWithFactoryMethodWithErrors()
  {
    var errorMessage = "Some conflict";
    var result = Result.Conflict(errorMessage);

    result.Value.Should().BeNull();
    result.Status.Should().Be(ResultStatus.Conflict);
    result.Errors.Single().Should().Be(errorMessage);
  }

  [Fact]
  public void InitializeUnavailableResultWithFactoryMethodWithErrors()
  {
    var errorMessage = "Something unavailable";
    var result = Result.Unavailable(errorMessage);

    result.Value.Should().BeNull();
    result.Status.Should().Be(ResultStatus.Unavailable);
    result.Errors.Single().Should().Be(errorMessage);
  }

  [Fact]
  public void InitializesCriticalErrorResultWithFactoryMethodWithErrors()
  {
    var errorMessage = "Some critical error";
    var result = Result.CriticalError(errorMessage);

    result.Value.Should().BeNull();
    result.Status.Should().Be(ResultStatus.CriticalError);
    result.Errors.Single().Should().Be(errorMessage);
  }
}
