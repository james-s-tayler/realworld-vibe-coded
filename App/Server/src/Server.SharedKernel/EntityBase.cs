using System.ComponentModel.DataAnnotations;

namespace Server.SharedKernel;

/// <summary>
/// A base class for DDD Entities. Includes support for domain events dispatched post-persistence.
/// All entities use Guid as their ID type.
/// </summary>
public abstract class EntityBase : HasDomainEventsBase, IAuditableEntity
{
  public Guid Id { get; set; }

  /// <summary>
  /// Row version for optimistic concurrency control
  /// </summary>
  [Timestamp]
  public byte[] ChangeCheck { get; set; } = Array.Empty<byte>();

  /// <summary>
  /// Timestamp when the entity was created (UTC)
  /// </summary>
  public DateTime CreatedAt { get; set; }

  /// <summary>
  /// Timestamp when the entity was last updated (UTC)
  /// </summary>
  public DateTime UpdatedAt { get; set; }

  /// <summary>
  /// Username of the user who created the entity
  /// </summary>
  public string CreatedBy { get; set; } = string.Empty;

  /// <summary>
  /// Username of the user who last updated the entity
  /// </summary>
  public string UpdatedBy { get; set; } = string.Empty;
}
