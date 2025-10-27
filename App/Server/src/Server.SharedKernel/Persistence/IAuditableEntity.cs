using System.ComponentModel.DataAnnotations;

namespace Server.SharedKernel.Persistence;

/// <summary>
/// Interface for entities that track creation and modification timestamps,
/// as well as optimistic concurrency control.
/// </summary>
public interface IAuditableEntity
{
  /// <summary>
  /// Row version for optimistic concurrency control
  /// </summary>
  [Timestamp]
  byte[] ChangeCheck { get; set; }

  /// <summary>
  /// Timestamp when the entity was created (UTC)
  /// </summary>
  DateTime CreatedAt { get; set; }

  /// <summary>
  /// Timestamp when the entity was last updated (UTC)
  /// </summary>
  DateTime UpdatedAt { get; set; }

  /// <summary>
  /// Username of the user who created the entity
  /// </summary>
  string CreatedBy { get; set; }

  /// <summary>
  /// Username of the user who last updated the entity
  /// </summary>
  string UpdatedBy { get; set; }
}
