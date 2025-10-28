using Server.SharedKernel.Result;

namespace Server.UnitTests.Result;

/// <summary>
/// Tests for ErrorDetail class to ensure proper construction and property assignment.
/// </summary>
public class ErrorDetailTests
{
  [Fact]
  public void DefaultConstructor_ShouldCreateEmptyErrorDetail()
  {
    // Act
    var errorDetail = new ErrorDetail();

    // Assert
    errorDetail.Identifier.ShouldBe(string.Empty);
    errorDetail.ErrorMessage.ShouldBe(string.Empty);
    errorDetail.ErrorCode.ShouldBe(string.Empty);
    errorDetail.Severity.ShouldBe(ValidationSeverity.Error);
  }

  [Fact]
  public void Constructor_WithErrorMessage_ShouldSetErrorMessage()
  {
    // Act
    var errorDetail = new ErrorDetail("Error occurred");

    // Assert
    errorDetail.ErrorMessage.ShouldBe("Error occurred");
    errorDetail.Identifier.ShouldBe(string.Empty);
    errorDetail.ErrorCode.ShouldBe(string.Empty);
    errorDetail.Severity.ShouldBe(ValidationSeverity.Error);
  }

  [Fact]
  public void Constructor_WithIdentifierAndMessage_ShouldSetBothProperties()
  {
    // Act
    var errorDetail = new ErrorDetail("fieldName", "Field is required");

    // Assert
    errorDetail.Identifier.ShouldBe("fieldName");
    errorDetail.ErrorMessage.ShouldBe("Field is required");
    errorDetail.ErrorCode.ShouldBe(string.Empty);
    errorDetail.Severity.ShouldBe(ValidationSeverity.Error);
  }

  [Fact]
  public void Constructor_WithAllParameters_ShouldSetAllProperties()
  {
    // Act
    var errorDetail = new ErrorDetail(
      "email",
      "Email is invalid",
      "EMAIL_INVALID",
      ValidationSeverity.Warning);

    // Assert
    errorDetail.Identifier.ShouldBe("email");
    errorDetail.ErrorMessage.ShouldBe("Email is invalid");
    errorDetail.ErrorCode.ShouldBe("EMAIL_INVALID");
    errorDetail.Severity.ShouldBe(ValidationSeverity.Warning);
  }

  [Fact]
  public void Properties_ShouldBeSettable()
  {
    // Arrange
    var errorDetail = new ErrorDetail();

    // Act
    errorDetail.Identifier = "username";
    errorDetail.ErrorMessage = "Username taken";
    errorDetail.ErrorCode = "USER_EXISTS";
    errorDetail.Severity = ValidationSeverity.Info;

    // Assert
    errorDetail.Identifier.ShouldBe("username");
    errorDetail.ErrorMessage.ShouldBe("Username taken");
    errorDetail.ErrorCode.ShouldBe("USER_EXISTS");
    errorDetail.Severity.ShouldBe(ValidationSeverity.Info);
  }
}
