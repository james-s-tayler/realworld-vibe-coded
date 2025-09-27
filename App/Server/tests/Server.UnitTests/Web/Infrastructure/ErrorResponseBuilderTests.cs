using Server.Web.Infrastructure;
using Shouldly;
using Xunit;

namespace Server.UnitTests.Web.Infrastructure;

public class ErrorResponseBuilderTests
{
  [Fact]
  public void CreateErrorResponse_SingleError_ReturnsCorrectFormat()
  {
    // Arrange
    const string errorMessage = "Test error";

    // Act
    var result = ErrorResponseBuilder.CreateErrorResponse(errorMessage);

    // Assert
    result.ShouldContain("\"errors\"");
    result.ShouldContain("\"body\"");
    result.ShouldContain(errorMessage);
  }

  [Fact]
  public void CreateErrorResponse_MultipleErrors_ReturnsCorrectFormat()
  {
    // Arrange
    var errors = new[] { "Error 1", "Error 2" };

    // Act
    var result = ErrorResponseBuilder.CreateErrorResponse(errors);

    // Assert
    result.ShouldContain("\"errors\"");
    result.ShouldContain("\"body\"");
    result.ShouldContain("Error 1");
    result.ShouldContain("Error 2");
  }

  [Fact]
  public void CreateUnauthorizedResponse_ReturnsCorrectFormat()
  {
    // Act
    var result = ErrorResponseBuilder.CreateUnauthorizedResponse();

    // Assert
    result.ShouldContain("\"errors\"");
    result.ShouldContain("\"body\"");
    result.ShouldContain("Unauthorized");
  }
}
