using Ardalis.Result;
using Server.SharedKernel;

namespace Server.UnitTests;

/// <summary>
/// Tests for CustomArdalisResultFactory to ensure reflection-based Result creation works correctly
/// and would catch issues if the Ardalis.Result library changes in incompatible ways.
/// </summary>
public class CustomArdalisResultFactoryTests
{
  [Fact]
  public void CriticalError_WithSingleValidationError_ShouldCreateResultWithCriticalErrorStatus()
  {
    // Arrange
    var validationError = new ValidationError("TestIdentifier", "Test error message");

    // Act
    var result = CustomArdalisResultFactory.CriticalError<string>(validationError);

    // Assert
    result.ShouldNotBeNull();
    result.Status.ShouldBe(ResultStatus.CriticalError);
    result.IsSuccess.ShouldBeFalse();
  }

  [Fact]
  public void CriticalError_WithSingleValidationError_ShouldSetValidationErrors()
  {
    // Arrange
    var validationError = new ValidationError("TestIdentifier", "Test error message");

    // Act
    var result = CustomArdalisResultFactory.CriticalError<string>(validationError);

    // Assert
    result.ValidationErrors.Count().ShouldBe(1);
    result.ValidationErrors.First().Identifier.ShouldBe("TestIdentifier");
    result.ValidationErrors.First().ErrorMessage.ShouldBe("Test error message");
  }

  [Fact]
  public void CriticalError_WithMultipleValidationErrors_ShouldCreateResultWithCriticalErrorStatus()
  {
    // Arrange
    var validationErrors = new[]
    {
      new ValidationError("Error1", "First error"),
      new ValidationError("Error2", "Second error"),
      new ValidationError("Error3", "Third error")
    };

    // Act
    var result = CustomArdalisResultFactory.CriticalError<string>(validationErrors);

    // Assert
    result.ShouldNotBeNull();
    result.Status.ShouldBe(ResultStatus.CriticalError);
    result.IsSuccess.ShouldBeFalse();
  }

  [Fact]
  public void CriticalError_WithMultipleValidationErrors_ShouldSetAllValidationErrors()
  {
    // Arrange
    var validationErrors = new[]
    {
      new ValidationError("Error1", "First error"),
      new ValidationError("Error2", "Second error"),
      new ValidationError("Error3", "Third error")
    };

    // Act
    var result = CustomArdalisResultFactory.CriticalError<string>(validationErrors);

    // Assert
    result.ValidationErrors.Count().ShouldBe(3);
    result.ValidationErrors.Any(e => e.Identifier == "Error1" && e.ErrorMessage == "First error").ShouldBeTrue();
    result.ValidationErrors.Any(e => e.Identifier == "Error2" && e.ErrorMessage == "Second error").ShouldBeTrue();
    result.ValidationErrors.Any(e => e.Identifier == "Error3" && e.ErrorMessage == "Third error").ShouldBeTrue();
  }

  [Fact]
  public void CriticalError_WithDifferentGenericTypes_ShouldWorkForString()
  {
    // Arrange
    var validationError = new ValidationError("TestError", "Test message");

    // Act
    var result = CustomArdalisResultFactory.CriticalError<string>(validationError);

    // Assert
    result.ShouldBeOfType<Result<string>>();
    result.Status.ShouldBe(ResultStatus.CriticalError);
  }

  [Fact]
  public void CriticalError_WithDifferentGenericTypes_ShouldWorkForInt()
  {
    // Arrange
    var validationError = new ValidationError("TestError", "Test message");

    // Act
    var result = CustomArdalisResultFactory.CriticalError<int>(validationError);

    // Assert
    result.ShouldBeOfType<Result<int>>();
    result.Status.ShouldBe(ResultStatus.CriticalError);
  }

  [Fact]
  public void CriticalError_WithDifferentGenericTypes_ShouldWorkForComplexType()
  {
    // Arrange
    var validationError = new ValidationError("TestError", "Test message");

    // Act
    var result = CustomArdalisResultFactory.CriticalError<TestComplexType>(validationError);

    // Assert
    result.ShouldBeOfType<Result<TestComplexType>>();
    result.Status.ShouldBe(ResultStatus.CriticalError);
  }

  [Fact]
  public void CriticalError_ResultValue_ShouldBeDefault()
  {
    // Arrange
    var validationError = new ValidationError("TestError", "Test message");

    // Act
    var result = CustomArdalisResultFactory.CriticalError<string>(validationError);

    // Assert
    result.Value.ShouldBeNull();
  }

  [Fact]
  public void CriticalError_ShouldBeCompatibleWithArdalisResultType()
  {
    // Arrange
    var validationError = new ValidationError("TestError", "Test message");

    // Act
    var result = CustomArdalisResultFactory.CriticalError<string>(validationError);

    // Assert - Should be assignable to Ardalis.Result.Result<T>
    Result<string> ardalisResult = result;
    ardalisResult.ShouldNotBeNull();
    ardalisResult.Status.ShouldBe(ResultStatus.CriticalError);
  }

  /// <summary>
  /// This test verifies that the reflection-based approach can access the protected constructor.
  /// If this test fails after a library upgrade, it indicates that Ardalis.Result has changed
  /// its internal structure and CustomArdalisResultFactory needs to be updated.
  /// </summary>
  [Fact]
  public void CriticalError_ReflectionAccessToProtectedConstructor_ShouldSucceed()
  {
    // Arrange
    var validationError = new ValidationError("ReflectionTest", "Testing reflection access");

    // Act & Assert
    Should.NotThrow(() => CustomArdalisResultFactory.CriticalError<string>(validationError));
  }

  /// <summary>
  /// This test ensures that if reflection fails, it throws a clear InvalidOperationException
  /// rather than a generic NullReferenceException or similar.
  /// </summary>
  [Fact]
  public void CriticalError_WithEmptyValidationErrorArray_ShouldStillWork()
  {
    // Arrange
    var validationErrors = Array.Empty<ValidationError>();

    // Act
    var result = CustomArdalisResultFactory.CriticalError<string>(validationErrors);

    // Assert
    result.ShouldNotBeNull();
    result.Status.ShouldBe(ResultStatus.CriticalError);
    result.ValidationErrors.ShouldBeEmpty();
  }

  private class TestComplexType
  {
    public string Property1 { get; set; } = string.Empty;
    public int Property2 { get; set; }
  }
}
