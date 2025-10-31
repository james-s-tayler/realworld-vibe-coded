using Server.SharedKernel.Result;

namespace Server.UnitTests.Result;

/// <summary>
/// Comprehensive tests for Result<T> class covering all factory methods and properties.
/// </summary>
public class ResultTests
{
  [Fact]
  public void ImplicitConversion_FromValue_ShouldCreateSuccessResult()
  {
    // Act
    Result<string> result = "test value";

    // Assert
    result.IsSuccess.ShouldBeTrue();
    result.Value.ShouldBe("test value");
    result.Status.ShouldBe(ResultStatus.Ok);
  }

  [Fact]
  public void ImplicitConversion_ToValue_ShouldReturnValue()
  {
    // Arrange
    var result = Result<string>.Success("test value");

    // Act
    string value = result;

    // Assert
    value.ShouldBe("test value");
  }

  [Fact]
  public void GetValue_ShouldReturnValue()
  {
    // Arrange
    var result = Result<string>.Success("test value");

    // Act
    var value = result.GetValue();

    // Assert
    value.ShouldBe("test value");
  }

  [Fact]
  public void Success_WithValue_ShouldCreateSuccessResult()
  {
    // Act
    var result = Result<string>.Success("test");

    // Assert
    result.IsSuccess.ShouldBeTrue();
    result.Status.ShouldBe(ResultStatus.Ok);
    result.Value.ShouldBe("test");
  }

  [Fact]
  public void Success_WithValueAndMessage_ShouldCreateSuccessResultWithMessage()
  {
    // Act
    var result = Result<string>.Success("test", "Operation completed");

    // Assert
    result.IsSuccess.ShouldBeTrue();
    result.Status.ShouldBe(ResultStatus.Ok);
    result.Value.ShouldBe("test");
    result.SuccessMessage.ShouldBe("Operation completed");
  }

  [Fact]
  public void Created_WithValue_ShouldCreateCreatedResult()
  {
    // Act
    var result = Result<string>.Created("new item");

    // Assert
    result.IsSuccess.ShouldBeTrue();
    result.Status.ShouldBe(ResultStatus.Created);
    result.Value.ShouldBe("new item");
  }

  [Fact]
  public void Created_WithValueAndLocation_ShouldCreateCreatedResultWithLocation()
  {
    // Act
    var result = Result<string>.Created("new item", "/api/items/123");

    // Assert
    result.IsSuccess.ShouldBeTrue();
    result.Status.ShouldBe(ResultStatus.Created);
    result.Value.ShouldBe("new item");
    result.Location.ShouldBe("/api/items/123");
  }

  [Fact]
  public void NoContent_ShouldCreateNoContentResult()
  {
    // Act
    var result = Result<string>.NoContent();

    // Assert
    result.IsSuccess.ShouldBeTrue();
    result.Status.ShouldBe(ResultStatus.NoContent);
  }

  [Fact]
  public void Invalid_WithSingleErrorDetail_ShouldCreateInvalidResult()
  {
    // Arrange
    var errorDetail = new ErrorDetail("field", "error message");

    // Act
    var result = Result<string>.Invalid(errorDetail);

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.Invalid);
    result.ErrorDetails.Count().ShouldBe(1);
    result.ErrorDetails.First().ShouldBe(errorDetail);
  }

  [Fact]
  public void Invalid_WithMultipleErrorDetails_ShouldCreateInvalidResult()
  {
    // Arrange
    var errorDetails = new[]
    {
      new ErrorDetail("field1", "error1"),
      new ErrorDetail("field2", "error2")
    };

    // Act
    var result = Result<string>.Invalid(errorDetails);

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.Invalid);
    result.ErrorDetails.Count().ShouldBe(2);
  }

  [Fact]
  public void Invalid_WithIEnumerableErrorDetails_ShouldCreateInvalidResult()
  {
    // Arrange
    var errorDetails = new List<ErrorDetail>
    {
      new ErrorDetail("field1", "error1"),
      new ErrorDetail("field2", "error2")
    };

    // Act
    var result = Result<string>.Invalid(errorDetails);

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.Invalid);
    result.ErrorDetails.Count().ShouldBe(2);
  }

  [Fact]
  public void NotFound_WithoutParameters_ShouldCreateNotFoundResult()
  {
    // Act
    var result = Result<string>.NotFound();

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.NotFound);
    result.ErrorDetails.ShouldBeEmpty();
  }

  [Fact]
  public void NotFound_WithGuid_ShouldCreateNotFoundResultWithErrorDetail()
  {
    // Arrange
    var id = Guid.NewGuid();

    // Act
    var result = Result<string>.NotFound(id);

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.NotFound);
    result.ErrorDetails.Count().ShouldBe(1);
    result.ErrorDetails.First().Identifier.ShouldBe("String");
    result.ErrorDetails.First().ErrorMessage.ShouldContain(id.ToString());
  }

  [Fact]
  public void NotFound_WithString_ShouldCreateNotFoundResultWithErrorDetail()
  {
    // Arrange
    var identifier = "test-slug";

    // Act
    var result = Result<string>.NotFound(identifier);

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.NotFound);
    result.ErrorDetails.Count().ShouldBe(1);
    result.ErrorDetails.First().Identifier.ShouldBe("String");
    result.ErrorDetails.First().ErrorMessage.ShouldContain(identifier);
  }

  [Fact]
  public void NotFound_WithTypeAndGuid_ShouldCreateNotFoundResultWithErrorDetail()
  {
    // Arrange
    var id = Guid.NewGuid();

    // Act
    var result = Result<string>.NotFound(typeof(int), id);

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.NotFound);
    result.ErrorDetails.Count().ShouldBe(1);
    result.ErrorDetails.First().Identifier.ShouldBe("Int32");
    result.ErrorDetails.First().ErrorMessage.ShouldBe($"Int32 identified by {id} was not found");
  }

  [Fact]
  public void NotFound_WithTypeAndString_ShouldCreateNotFoundResultWithErrorDetail()
  {
    // Arrange
    var identifier = "test-slug";

    // Act
    var result = Result<string>.NotFound(typeof(int), identifier);

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.NotFound);
    result.ErrorDetails.Count().ShouldBe(1);
    result.ErrorDetails.First().Identifier.ShouldBe("Int32");
    result.ErrorDetails.First().ErrorMessage.ShouldBe($"Int32 identified by {identifier} was not found");
  }

  [Fact]
  public void Forbidden_WithoutParameters_ShouldCreateForbiddenResult()
  {
    // Act
    var result = Result<string>.Forbidden();

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.Forbidden);
    result.ErrorDetails.ShouldBeEmpty();
  }

  [Fact]
  public void Forbidden_WithErrorDetails_ShouldCreateForbiddenResultWithErrors()
  {
    // Arrange
    var errorDetails = new[] { new ErrorDetail("permission", "denied") };

    // Act
    var result = Result<string>.Forbidden(errorDetails);

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.Forbidden);
    result.ErrorDetails.Count().ShouldBe(1);
  }

  [Fact]
  public void Unauthorized_WithoutParameters_ShouldCreateUnauthorizedResult()
  {
    // Act
    var result = Result<string>.Unauthorized();

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.Unauthorized);
    result.ErrorDetails.ShouldBeEmpty();
  }

  [Fact]
  public void Unauthorized_WithErrorDetails_ShouldCreateUnauthorizedResultWithErrors()
  {
    // Arrange
    var errorDetails = new[] { new ErrorDetail("auth", "not authenticated") };

    // Act
    var result = Result<string>.Unauthorized(errorDetails);

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.Unauthorized);
    result.ErrorDetails.Count().ShouldBe(1);
  }

  [Fact]
  public void Error_WithErrorDetails_ShouldCreateErrorResult()
  {
    // Arrange
    var errorDetails = new[] { new ErrorDetail("error", "something went wrong") };

    // Act
    var result = Result<string>.Error(errorDetails);

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.Error);
    result.ErrorDetails.Count().ShouldBe(1);
  }

  [Fact]
  public void Conflict_WithoutParameters_ShouldCreateConflictResult()
  {
    // Act
    var result = Result<string>.Conflict();

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.Conflict);
    result.ErrorDetails.ShouldBeEmpty();
  }

  [Fact]
  public void Conflict_WithErrorDetailsArray_ShouldCreateConflictResultWithErrors()
  {
    // Arrange
    var errorDetails = new[] { new ErrorDetail("conflict", "resource conflict") };

    // Act
    var result = Result<string>.Conflict(errorDetails);

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.Conflict);
    result.ErrorDetails.Count().ShouldBe(1);
  }

  [Fact]
  public void Conflict_WithIEnumerableErrorDetails_ShouldCreateConflictResultWithErrors()
  {
    // Arrange
    var errorDetails = new List<ErrorDetail> { new ErrorDetail("conflict", "resource conflict") };

    // Act
    var result = Result<string>.Conflict(errorDetails);

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.Conflict);
    result.ErrorDetails.Count().ShouldBe(1);
  }

  [Fact]
  public void Conflict_WithException_ShouldCreateConflictResultWithExceptionDetails()
  {
    // Arrange
    var exception = new InvalidOperationException("Conflict occurred");

    // Act
    var result = Result<string>.Conflict(exception);

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.Conflict);
    result.ErrorDetails.Count().ShouldBe(1);
    result.ErrorDetails.First().Identifier.ShouldBe("InvalidOperationException");
    result.ErrorDetails.First().ErrorMessage.ShouldBe("Conflict occurred");
  }

  [Fact]
  public void CriticalError_WithErrorDetails_ShouldCreateCriticalErrorResult()
  {
    // Arrange
    var errorDetails = new[] { new ErrorDetail("critical", "critical error") };

    // Act
    var result = Result<string>.CriticalError(errorDetails);

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.CriticalError);
    result.ErrorDetails.Count().ShouldBe(1);
  }

  [Fact]
  public void CriticalError_WithIEnumerableErrorDetails_ShouldCreateCriticalErrorResult()
  {
    // Arrange
    var errorDetails = new List<ErrorDetail> { new ErrorDetail("critical", "critical error") };

    // Act
    var result = Result<string>.CriticalError(errorDetails);

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.CriticalError);
    result.ErrorDetails.Count().ShouldBe(1);
  }

  [Fact]
  public void CriticalError_WithException_ShouldCreateCriticalErrorResultWithExceptionDetails()
  {
    // Arrange
    var exception = new Exception("Critical error");

    // Act
    var result = Result<string>.CriticalError(exception);

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.CriticalError);
    result.ErrorDetails.Count().ShouldBe(1);
    result.ErrorDetails.First().Identifier.ShouldBe("Exception");
    result.ErrorDetails.First().ErrorMessage.ShouldBe("Critical error");
  }

  [Fact]
  public void Unavailable_WithErrorDetails_ShouldCreateUnavailableResult()
  {
    // Arrange
    var errorDetails = new[] { new ErrorDetail("service", "service unavailable") };

    // Act
    var result = Result<string>.Unavailable(errorDetails);

    // Assert
    result.IsSuccess.ShouldBeFalse();
    result.Status.ShouldBe(ResultStatus.Unavailable);
    result.ErrorDetails.Count().ShouldBe(1);
  }

  [Fact]
  public void IsSuccess_ShouldReturnTrueForOkStatus()
  {
    // Arrange
    var result = Result<string>.Success("test");

    // Act & Assert
    result.IsSuccess.ShouldBeTrue();
  }

  [Fact]
  public void IsSuccess_ShouldReturnTrueForNoContentStatus()
  {
    // Arrange
    var result = Result<string>.NoContent();

    // Act & Assert
    result.IsSuccess.ShouldBeTrue();
  }

  [Fact]
  public void IsSuccess_ShouldReturnTrueForCreatedStatus()
  {
    // Arrange
    var result = Result<string>.Created("test");

    // Act & Assert
    result.IsSuccess.ShouldBeTrue();
  }

  [Fact]
  public void IsSuccess_ShouldReturnFalseForErrorStatus()
  {
    // Arrange
    var result = Result<string>.Error(new ErrorDetail("error", "error"));

    // Act & Assert
    result.IsSuccess.ShouldBeFalse();
  }
}
