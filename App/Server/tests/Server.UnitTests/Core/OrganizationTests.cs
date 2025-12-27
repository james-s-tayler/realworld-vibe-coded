using Server.Core.OrganizationAggregate;
using Server.SharedKernel.Persistence;

namespace Server.UnitTests.Core;

public class OrganizationTests
{
  [Fact]
  public void Constructor_WithValidNameAndIdentifier_CreatesOrganization()
  {
    // Arrange
    var name = "Test Organization";
    var identifier = "test-org";

    // Act
    var organization = new Organization(name, identifier);

    // Assert
    organization.Name.ShouldBe(name);
    organization.Identifier.ShouldBe(identifier);
  }

  [Fact]
  public void Constructor_WithNullName_ThrowsArgumentNullException()
  {
    // Arrange
    string? name = null;
    var identifier = "test-org";

    // Act & Assert
    Should.Throw<ArgumentNullException>(() => new Organization(name!, identifier));
  }

  [Fact]
  public void Constructor_WithEmptyName_ThrowsArgumentException()
  {
    // Arrange
    var name = string.Empty;
    var identifier = "test-org";

    // Act & Assert
    Should.Throw<ArgumentException>(() => new Organization(name, identifier));
  }

  [Fact]
  public void Constructor_WithNullIdentifier_ThrowsArgumentNullException()
  {
    // Arrange
    var name = "Test Organization";
    string? identifier = null;

    // Act & Assert
    Should.Throw<ArgumentNullException>(() => new Organization(name, identifier!));
  }

  [Fact]
  public void Constructor_WithEmptyIdentifier_ThrowsArgumentException()
  {
    // Arrange
    var name = "Test Organization";
    var identifier = string.Empty;

    // Act & Assert
    Should.Throw<ArgumentException>(() => new Organization(name, identifier));
  }

  [Fact]
  public void Organization_InheritsFromEntityBase()
  {
    // Arrange & Act
    var organization = new Organization("Test", "test");

    // Assert
    organization.ShouldBeAssignableTo<EntityBase>();
  }

  [Fact]
  public void Organization_ImplementsIAggregateRoot()
  {
    // Arrange & Act
    var organization = new Organization("Test", "test");

    // Assert
    organization.ShouldBeAssignableTo<IAggregateRoot>();
  }
}
