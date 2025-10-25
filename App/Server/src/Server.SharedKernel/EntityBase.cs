using System.ComponentModel.DataAnnotations;

namespace Server.SharedKernel;

/// <summary>
/// A base class for DDD Entities. Includes support for domain events dispatched post-persistence.
/// If you prefer GUID Ids, change it here.
/// If you need to support both GUID and int IDs, change to EntityBase&lt;TId&gt; and use TId as the type for Id.
/// </summary>
public abstract class EntityBase : HasDomainEventsBase, IAuditableEntity
{
  public int Id { get; set; }

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
  public string? CreatedBy { get; set; }

  /// <summary>
  /// Username of the user who last updated the entity
  /// </summary>
  public string? UpdatedBy { get; set; }
}

public abstract class EntityBase<TId> : HasDomainEventsBase, IAuditableEntity
  where TId : struct, IEquatable<TId>
{
  public TId Id { get; set; } = default!;

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
  public string? CreatedBy { get; set; }

  /// <summary>
  /// Username of the user who last updated the entity
  /// </summary>
  public string? UpdatedBy { get; set; }
}

/// <summary>
/// For use with Vogen or similar tools for generating code for 
/// strongly typed Ids.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TId"></typeparam>
public abstract class EntityBase<T, TId> : HasDomainEventsBase, IAuditableEntity
  where T : EntityBase<T, TId>
{
  public TId Id { get; set; } = default!;

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
  public string? CreatedBy { get; set; }

  /// <summary>
  /// Username of the user who last updated the entity
  /// </summary>
  public string? UpdatedBy { get; set; }
}
