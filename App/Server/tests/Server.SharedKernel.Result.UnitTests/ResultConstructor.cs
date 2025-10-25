using Ardalis.Result;
using FluentAssertions;
using Xunit;
using Result = Ardalis.Result.Result;

namespace Server.SharedKernel.ResultUnitTests;

public class ResultConstructor
{
  private class TestObject
  {
  }

  [Fact]
  public void InitializesStronglyTypedStringValue()
  {
    string expectedString = "test string";
    var result = new Result<string>(expectedString);

    result.Value.Should().Be(expectedString);
  }

  [Fact]
  public void InitializesStronglyTypedIntValue()
  {
    int expectedInt = 123;
    var result = new Result<int>(expectedInt);

    result.Value.Should().Be(expectedInt);
  }

  [Fact]
  public void InitializesStronglyTypedObjectValue()
  {
    var expectedObject = new TestObject();
    var result = new Result<TestObject>(expectedObject);

    result.Value.Should().Be(expectedObject);
  }

  [Fact]
  public void InitializesValueToNullGivenNullConstructorArgument()
  {
    var result = new Result<object>(null);

    result.Value.Should().BeNull();
  }

  [Theory]
  [InlineData(null)]
  [InlineData(123)]
  [InlineData("test value")]
  public void InitializesStatusToOkGivenValue(object value)
  {
    var result = new Result<object>(value);

    result.Status.Should().Be(ResultStatus.Ok);
  }

  [Theory]
  [InlineData(null)]
  [InlineData(123)]
  [InlineData("test value")]
  public void InitializesValueUsingFactoryMethodAndSetsStatusToOk(object value)
  {
    var result = Result<object>.Success(value);

    result.Status.Should().Be(ResultStatus.Ok);
    result.Value.Should().Be(value);
  }

  [Theory]
  [InlineData(null)]
  [InlineData(123)]
  [InlineData("test value")]
  public void InitializesValueUsingGenericFactoryMethodAndSetsStatusToOk(object value)
  {
    var result = Result.Success(value);

    result.Status.Should().Be(ResultStatus.Ok);
    result.Value.Should().Be(value);
  }

  [Theory]
  [InlineData(null)]
  [InlineData(123)]
  [InlineData("test value")]
  public void InitializesValueUsingGenericFactoryMethodAndSetsStatusToOkWithMessage(object value)
  {
    var message = "success";
    var result = Result.Success(value, message);

    result.Status.Should().Be(ResultStatus.Ok);
    result.Value.Should().Be(value);
    result.SuccessMessage.Should().Be(message);
  }

  [Theory]
  [InlineData(null)]
  [InlineData(123)]
  [InlineData("test value")]
  public void InitializesStatusToCreatedAndSetLocationGivenCreatedFactoryCall(object value)
  {
    string location = "https://github.com/ardalis/Result";
    var result = Result<object>.Created(value, location);

    result.Status.Should().Be(ResultStatus.Created);
    result.Location.Should().Be(location);
    result.IsSuccess.Should().BeTrue();
  }

  [Theory]
  [InlineData(null)]
  [InlineData(123)]
  [InlineData("test value")]
  public void InitializesStatusToCreatedGivenCreatedFactoryCall(object value)
  {
    var result = Result<object>.Created(value);

    result.Status.Should().Be(ResultStatus.Created);
    string.Empty.Should().Be(result.Location);
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public void InitializesStatusToErrorGivenErrorFactoryCall()
  {
    var result = Result<object>.Error();

    result.Status.Should().Be(ResultStatus.Error);
  }

  [Fact]
  public void InitializesStatusToErrorGivenErrorFactoryCallWithSimpleMessage()
  {
    string errorMessage = Guid.NewGuid().ToString();
    var result = Result<object>.Error(errorMessage);

    result.Status.Should().Be(ResultStatus.Error);
    result.Errors.Single().Should().Be(errorMessage);
  }

  [Fact]
  public void InitializesStatusToErrorAndSetsErrorMessageGivenErrorFactoryCall()
  {
    string errorMessage = "Something bad happened.";
    string correlationId = Guid.NewGuid().ToString();
    ErrorList errors = new(new[] { errorMessage }, correlationId);
    var result = Result<object>.Error(errors);

    result.Status.Should().Be(ResultStatus.Error);
    result.Errors.Single().Should().Be(errorMessage);
    result.CorrelationId.Should().Be(correlationId);
  }

  [Fact]
  public void InitializesStatusToErrorAndSetsErrorMessageGivenErrorFactoryCallWithoutCorrelationId()
  {
    string errorMessage = "Something bad happened.";
    ErrorList errors = new(new[] { errorMessage });
    var result = Result<object>.Error(errors);

    result.Status.Should().Be(ResultStatus.Error);
    result.Errors.Single().Should().Be(errorMessage);
  }

  [Fact]
  public void InitializesStatusToInvalidAndSetsErrorMessagesGivenInvalidFactoryCall()
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
    // TODO: Support duplicates of the same key with multiple errors
    var result = Result<object>.Invalid(validationErrors);

    result.Status.Should().Be(ResultStatus.Invalid);
    result.ValidationErrors.Should().ContainEquivalentOf(new ValidationError { ErrorMessage = "Name is required", Identifier = "name" });
    result.ValidationErrors.Should().ContainEquivalentOf(new ValidationError { ErrorMessage = "PostalCode cannot exceed 10 characters", Identifier = "postalCode" });
  }

  [Fact]
  public void InitializesStatusToNotFoundGivenNotFoundFactoryCall()
  {
    var result = Result<object>.NotFound();

    result.Status.Should().Be(ResultStatus.NotFound);
    result.Errors.Should().BeEmpty();
  }

  [Fact]
  public void InitializesStatusToNotFoundGivenNotFoundFactoryCallWithString()
  {
    var errorMessage = "User Not Found";
    var result = Result<object>.NotFound(errorMessage);

    result.Status.Should().Be(ResultStatus.NotFound);
    result.Errors.First().Should().Be(errorMessage);
  }

  [Fact]
  public void InitializesStatusToConflictGivenConflictFactoryCall()
  {
    var result = Result<object>.Conflict();

    result.Status.Should().Be(ResultStatus.Conflict);
    result.Errors.Should().BeEmpty();
  }

  [Fact]
  public void InitializesStatusToConflictGivenConflictFactoryCallWithString()
  {
    var errorMessage = "Some conflict";
    var result = Result<object>.Conflict(errorMessage);

    result.Status.Should().Be(ResultStatus.Conflict);
    result.Errors.Single().Should().Be(errorMessage);
  }

  [Fact]
  public void InitializesStatusToForbiddenGivenForbiddenFactoryCall()
  {
    var result = Result<object>.Forbidden();

    result.Status.Should().Be(ResultStatus.Forbidden);
  }

  [Fact]
  public void InitializesStatusToForbiddenGivenForbiddenFactoryCallWithString()
  {
    var errorMessage = "You are forbidden";
    var result = Result<object>.Forbidden(errorMessage);

    result.Status.Should().Be(ResultStatus.Forbidden);
    result.Errors.Single().Should().Be(errorMessage);
  }

  [Fact]
  public void InitializesStatusToUnauthorizedGivenUnauthorizedFactoryCall()
  {
    var result = Result<object>.Unauthorized();

    result.Status.Should().Be(ResultStatus.Unauthorized);
  }

  [Fact]
  public void InitializesStatusToUnauthorizedGivenUnauthorizedFactoryCallWithString()
  {
    var errorMessage = "You are unauthorized";
    var result = Result<object>.Unauthorized(errorMessage);

    result.Status.Should().Be(ResultStatus.Unauthorized);
    result.Errors.Single().Should().Be(errorMessage);
  }

  [Fact]
  public void InitializesStatusToUnavailableGivenUnavailableFactoryCallWithString()
  {
    var errorMessage = "Service Unavailable";
    var result = Result<object>.Unavailable(errorMessage);

    result.Status.Should().Be(ResultStatus.Unavailable);
    result.Errors.First().Should().Be(errorMessage);
  }


  [Fact]
  public void InitializedIsSuccessTrueForSuccessFactoryCall()
  {
    var result = Result<object>.Success(new object());

    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public void InitializedIsSuccessFalseForErrorFactoryCall()
  {
    var result = Result<object>.Error();

    result.IsSuccess.Should().BeFalse();
  }

  [Fact]
  public void InitializedIsSuccessFalseForForbiddenFactoryCall()
  {
    var result = Result<object>.Forbidden();

    result.IsSuccess.Should().BeFalse();
  }

  [Fact]
  public void InitializedIsSuccessFalseForInvalidListFactoryCall()
  {
    var result = Result<object>.Invalid(new List<ValidationError>());

    result.IsSuccess.Should().BeFalse();
  }

  [Fact]
  public void InitializedIsSuccessFalseForInvalidFactoryCall()
  {
    var result = Result<object>.Invalid(new ValidationError());

    result.IsSuccess.Should().BeFalse();
  }

  [Fact]
  public void InitializedIsSuccessFalseForNotFoundFactoryCall()
  {
    var result = Result<object>.NotFound();

    result.IsSuccess.Should().BeFalse();
  }

  [Fact]
  public void InitializedIsSuccessFalseForConflictFactoryCall()
  {
    var result = Result<object>.Conflict();

    result.IsSuccess.Should().BeFalse();
  }

  [Fact]
  public void InitializedIsSuccessFalseForCriticalErrorFactoryCall()
  {
    var result = Result<object>.CriticalError();

    result.IsSuccess.Should().BeFalse();
  }

  [Fact]
  public void InitializesStatusToCriticalErrorGivenCriticalErrorFactoryCallWithString()
  {
    var errorMessage = "Critical system error";
    var result = Result<object>.CriticalError(errorMessage);

    result.Status.Should().Be(ResultStatus.CriticalError);
    result.Errors.First().Should().Be(errorMessage);
    result.IsSuccess.Should().BeFalse();
  }

  [Fact]
  public void InitializesStatusToCriticalErrorAndSetsValidationErrorGivenCriticalErrorFactoryCall()
  {
    var exception = new InvalidOperationException("An unexpected error occurred");
    var result = Server.SharedKernel.CustomArdalisResultFactory.CriticalError<object>(exception);

    result.Status.Should().Be(ResultStatus.CriticalError);
    result.ValidationErrors.Should().HaveCount(1);
    result.ValidationErrors.Should().ContainEquivalentOf(new ValidationError { Identifier = "InvalidOperationException", ErrorMessage = "An unexpected error occurred" });
    result.IsSuccess.Should().BeFalse();
  }

  [Fact]
  public void InitializesStatusToNoContentForNoContentFactoryCall()
  {
    var result = Result<object>.NoContent();

    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public void InitializedIsSuccessTrueForCreatedFactoryCall()
  {
    var result = Result<object>.Created(new object());

    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public void InitializedIsSuccessTrueForCreatedWithLocationFactoryCall()
  {
    var result = Result<object>.Created(new object(), "sample/endpoint");

    result.IsSuccess.Should().BeTrue();
  }
}
