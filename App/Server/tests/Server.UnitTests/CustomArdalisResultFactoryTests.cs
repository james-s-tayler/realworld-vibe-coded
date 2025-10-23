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
  public void CriticalError_WithException_ShouldCreateResultWithCriticalErrorStatus()
  {
    // Arrange
    var exception = new InvalidOperationException("Test error message");

    // Act
    var result = CustomArdalisResultFactory.CriticalError<string>(exception);

    // Assert
    result.ShouldNotBeNull();
    result.Status.ShouldBe(ResultStatus.CriticalError);
    result.IsSuccess.ShouldBeFalse();
  }

  [Fact]
  public void CriticalError_WithException_ShouldSetValidationErrors()
  {
    // Arrange
    var exception = new InvalidOperationException("Test error message");

    // Act
    var result = CustomArdalisResultFactory.CriticalError<string>(exception);

    // Assert
    result.ValidationErrors.Count().ShouldBe(1);
    result.ValidationErrors.First().Identifier.ShouldBe("InvalidOperationException");
    result.ValidationErrors.First().ErrorMessage.ShouldBe("Test error message");
  }

  [Fact]
  public void CriticalError_WithDifferentGenericTypes_ShouldWorkForString()
  {
    // Arrange
    var exception = new Exception("Test message");

    // Act
    var result = CustomArdalisResultFactory.CriticalError<string>(exception);

    // Assert
    result.ShouldBeOfType<Result<string>>();
    result.Status.ShouldBe(ResultStatus.CriticalError);
  }

  [Fact]
  public void CriticalError_WithDifferentGenericTypes_ShouldWorkForInt()
  {
    // Arrange
    var exception = new Exception("Test message");

    // Act
    var result = CustomArdalisResultFactory.CriticalError<int>(exception);

    // Assert
    result.ShouldBeOfType<Result<int>>();
    result.Status.ShouldBe(ResultStatus.CriticalError);
  }

  [Fact]
  public void CriticalError_WithDifferentGenericTypes_ShouldWorkForComplexType()
  {
    // Arrange
    var exception = new Exception("Test message");

    // Act
    var result = CustomArdalisResultFactory.CriticalError<TestComplexType>(exception);

    // Assert
    result.ShouldBeOfType<Result<TestComplexType>>();
    result.Status.ShouldBe(ResultStatus.CriticalError);
  }

  [Fact]
  public void CriticalError_ResultValue_ShouldBeDefault()
  {
    // Arrange
    var exception = new Exception("Test message");

    // Act
    var result = CustomArdalisResultFactory.CriticalError<string>(exception);

    // Assert
    result.Value.ShouldBeNull();
  }

  [Fact]
  public void CriticalError_ShouldBeCompatibleWithArdalisResultType()
  {
    // Arrange
    var exception = new Exception("Test message");

    // Act
    var result = CustomArdalisResultFactory.CriticalError<string>(exception);

    // Assert - Should be assignable to Ardalis.Result.Result<T>
    Result<string> ardalisResult = result;
    ardalisResult.ShouldNotBeNull();
    ardalisResult.Status.ShouldBe(ResultStatus.CriticalError);
  }

  [Fact]
  public void CriticalError_ReflectionAccessToProtectedConstructor_ShouldSucceed()
  {
    // Arrange
    var exception = new Exception("Testing reflection access");

    // Act & Assert
    Should.NotThrow(() => CustomArdalisResultFactory.CriticalError<string>(exception));
  }

  [Fact]
  public void Conflict_WithException_ShouldCreateResultWithConflictStatus()
  {
    // Arrange
    var exception = new InvalidOperationException("Conflict occurred");

    // Act
    var result = CustomArdalisResultFactory.Conflict<string>(exception);

    // Assert
    result.ShouldNotBeNull();
    result.Status.ShouldBe(ResultStatus.Conflict);
    result.IsSuccess.ShouldBeFalse();
  }

  [Fact]
  public void Conflict_WithException_ShouldSetValidationErrors()
  {
    // Arrange
    var exception = new InvalidOperationException("Conflict message");

    // Act
    var result = CustomArdalisResultFactory.Conflict<string>(exception);

    // Assert
    result.ValidationErrors.Count().ShouldBe(1);
    result.ValidationErrors.First().Identifier.ShouldBe("InvalidOperationException");
    result.ValidationErrors.First().ErrorMessage.ShouldBe("Conflict message");
  }

  [Fact]
  public void Conflict_WithDifferentGenericTypes_ShouldWorkForString()
  {
    // Arrange
    var exception = new Exception("Conflict");

    // Act
    var result = CustomArdalisResultFactory.Conflict<string>(exception);

    // Assert
    result.ShouldBeOfType<Result<string>>();
    result.Status.ShouldBe(ResultStatus.Conflict);
  }

  [Fact]
  public void Conflict_WithDifferentGenericTypes_ShouldWorkForInt()
  {
    // Arrange
    var exception = new Exception("Conflict");

    // Act
    var result = CustomArdalisResultFactory.Conflict<int>(exception);

    // Assert
    result.ShouldBeOfType<Result<int>>();
    result.Status.ShouldBe(ResultStatus.Conflict);
  }

  [Fact]
  public void Conflict_WithDifferentGenericTypes_ShouldWorkForComplexType()
  {
    // Arrange
    var exception = new Exception("Conflict");

    // Act
    var result = CustomArdalisResultFactory.Conflict<TestComplexType>(exception);

    // Assert
    result.ShouldBeOfType<Result<TestComplexType>>();
    result.Status.ShouldBe(ResultStatus.Conflict);
  }

  [Fact]
  public void Conflict_ResultValue_ShouldBeDefault()
  {
    // Arrange
    var exception = new Exception("Conflict");

    // Act
    var result = CustomArdalisResultFactory.Conflict<string>(exception);

    // Assert
    result.Value.ShouldBeNull();
  }

  [Fact]
  public void Conflict_ShouldBeCompatibleWithArdalisResultType()
  {
    // Arrange
    var exception = new Exception("Conflict");

    // Act
    var result = CustomArdalisResultFactory.Conflict<string>(exception);

    // Assert - Should be assignable to Ardalis.Result.Result<T>
    Result<string> ardalisResult = result;
    ardalisResult.ShouldNotBeNull();
    ardalisResult.Status.ShouldBe(ResultStatus.Conflict);
  }

  /// <summary>
  /// This test validates that all reflection-based invariants required by CustomArdalisResultFactory
  /// continue to hold. If this test fails after a library upgrade, it indicates that Ardalis.Result
  /// has changed in an incompatible way and CustomArdalisResultFactory needs to be updated.
  /// 
  /// Validates:
  /// - Protected constructor Result(ResultStatus) exists and is accessible
  /// - ValidationErrors property exists
  /// - ValidationErrors property has a protected setter
  /// </summary>
  [Fact]
  public void ReflectionInvariants_ShouldBeValid()
  {
    // Arrange
    var resultType = typeof(Result<string>);

    // Act & Assert - Verify protected constructor exists
    var constructor = resultType.GetConstructor(
      System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
      null,
      new[] { typeof(ResultStatus) },
      null
    );
    constructor.ShouldNotBeNull("Ardalis.Result.Result<T> must have a protected constructor that accepts ResultStatus");

    // Act & Assert - Verify ValidationErrors property exists
    var validationErrorsProp = resultType.GetProperty(nameof(Result<string>.ValidationErrors));
    validationErrorsProp.ShouldNotBeNull("Ardalis.Result.Result<T> must have a ValidationErrors property");

    // Act & Assert - Verify ValidationErrors property has a protected setter
    var setter = validationErrorsProp.GetSetMethod(nonPublic: true);
    setter.ShouldNotBeNull("Ardalis.Result.Result<T>.ValidationErrors must have a protected setter");
  }

  private class TestComplexType
  {
    public string Property1 { get; set; } = string.Empty;
    public int Property2 { get; set; }
  }
}
