using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using MultiTenantPocApi.Data;

namespace MultiTenantPocApi.Endpoints;

/// <summary>
/// List articles response
/// </summary>
public class ArticlesListResponse
{
    public List<ArticleResponse> Articles { get; set; } = new();
    public int ArticlesCount { get; set; }
}

/// <summary>
/// List Articles endpoint - demonstrates tenant-scoped querying
/// Finbuckle automatically filters to current tenant's articles
/// </summary>
public class ListArticles : EndpointWithoutRequest<ArticlesListResponse>
{
    private readonly PocDbContext _db;

    public ListArticles(PocDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Get("/api/articles");
        // Require authentication - user must be logged in with TenantId claim
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // DEBUG: Log what TenantInfo the DbContext has
        Console.WriteLine($"[ListArticles] DbContext.TenantInfo: {(_db.TenantInfo != null ? $"{_db.TenantInfo.Id} ({_db.TenantInfo.Name})" : "NULL")}");
        
        // Finbuckle automatically applies tenant filter - only returns articles for current tenant
        var articles = await _db.Articles
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);

        Console.WriteLine($"[ListArticles] Found {articles.Count} articles");
        foreach (var art in articles)
        {
            Console.WriteLine($"  - {art.Title} (TenantId: {art.TenantId})");
        }

        await Send.OkAsync(new ArticlesListResponse
        {
            Articles = articles.Select(a => new ArticleResponse
            {
                Id = a.Id,
                Title = a.Title,
                Body = a.Body,
                TenantId = a.TenantId ?? "unknown",
                CreatedAt = a.CreatedAt
            }).ToList(),
            ArticlesCount = articles.Count
        }, ct);
    }
}
