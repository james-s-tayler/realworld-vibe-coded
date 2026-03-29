---
paths:
  - "App/Server/**"
---

### EF Core Entity Configuration

```csharp
// File: App/Server/src/Server.Infrastructure/Data/Config/{Entity}Configuration.cs
using Server.Core.{Aggregate};

namespace Server.Infrastructure.Data.Config;

public class {Entity}Configuration : IEntityTypeConfiguration<{Entity}>
{
  public void Configure(EntityTypeBuilder<{Entity}> builder)
  {
    // Property constraints
    builder.Property(x => x.Title)
      .HasMaxLength({Entity}.TitleMaxLength)
      .IsRequired();

    builder.Property(x => x.Slug)
      .HasMaxLength({Entity}.SlugMaxLength)
      .IsRequired();

    // Unique indexes
    builder.HasIndex(x => x.Slug).IsUnique();

    // Relationships
    builder.HasOne(x => x.Author)
      .WithMany()
      .HasForeignKey(x => x.AuthorId)
      .OnDelete(DeleteBehavior.Restrict);

    // Many-to-many
    builder.HasMany(x => x.Tags)
      .WithMany(x => x.Articles)
      .UsingEntity(j => j.ToTable("ArticleTags"));
  }
}
```

### ResponseMapper

```csharp
// File: App/Server/src/Server.Web/{Feature}/{Feature}Mapper.cs
using Server.Core.{Aggregate};

namespace Server.Web.{Feature};

public class {Feature}Mapper : ResponseMapper<{Feature}Response, {Entity}>
{
  public override Task<{Feature}Response> FromEntityAsync({Entity} entity, CancellationToken ct)
  {
    var response = new {Feature}Response
    {
      {Feature} = new {Feature}Dto
      {
        // Map only the fields SPEC-REFERENCE.md requires
        Title = entity.Title,
        Slug = entity.Slug,
        Description = entity.Description,
        Body = entity.Body,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
      },
    };

    return Task.FromResult(response);
  }
}
```
