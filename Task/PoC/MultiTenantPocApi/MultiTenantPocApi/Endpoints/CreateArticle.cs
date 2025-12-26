using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using MultiTenantPocApi.Data;
using MultiTenantPocApi.Models;

namespace MultiTenantPocApi.Endpoints;

/// <summary>
/// Create article request/response DTOs
/// </summary>
public class CreateArticleRequest
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}

public class ArticleResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Create Article endpoint - demonstrates tenant-scoped data creation
/// Mimics the real app's endpoint structure with FastEndpoints
/// </summary>
public class CreateArticle : Endpoint<CreateArticleRequest, ArticleResponse>
{
    private readonly PocDbContext _db;

    public CreateArticle(PocDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Post("/api/articles");
        AllowAnonymous(); // For POC simplicity
    }

    public override async Task HandleAsync(CreateArticleRequest req, CancellationToken ct)
    {
        var article = new Article
        {
            Id = Guid.NewGuid(),
            Title = req.Title,
            Body = req.Body,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
            // TenantId is automatically set by Finbuckle on SaveChanges
        };

        _db.Articles.Add(article);
        await _db.SaveChangesAsync(ct);

        await SendOkAsync(new ArticleResponse
        {
            Id = article.Id,
            Title = article.Title,
            Body = article.Body,
            TenantId = article.TenantId ?? "unknown",
            CreatedAt = article.CreatedAt
        }, ct);
    }
}
