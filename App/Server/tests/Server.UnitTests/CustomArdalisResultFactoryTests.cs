using Server.SharedKernel.Ardalis.Result;
using Server.SharedKernel.Result;

namespace Server.UnitTests;

/// <summary>
/// Tests for CustomArdalisResultFactory to ensure Result creation works correctly.
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
  public void CriticalError_ShouldBeCompatibleWithResultType()
  {
    // Arrange
    var exception = new Exception("Test message");

    // Act
    var result = CustomArdalisResultFactory.CriticalError<string>(exception);

    // Assert - Should be assignable to Result<T>
    Result<string> resultType = result;
    resultType.ShouldNotBeNull();
    resultType.Status.ShouldBe(ResultStatus.CriticalError);
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
  public void Conflict_ShouldBeCompatibleWithResultType()
  {
    // Arrange
    var exception = new Exception("Conflict");

    // Act
    var result = CustomArdalisResultFactory.Conflict<string>(exception);

    // Assert - Should be assignable to Result<T>
    Result<string> resultType = result;
    resultType.ShouldNotBeNull();
    resultType.Status.ShouldBe(ResultStatus.Conflict);
  }

  private class TestComplexType
  {
    public string Property1 { get; set; } = string.Empty;
    public int Property2 { get; set; }
  }
}
