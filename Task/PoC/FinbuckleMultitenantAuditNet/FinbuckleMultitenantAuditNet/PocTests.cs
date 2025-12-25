using Microsoft.EntityFrameworkCore;

namespace FinbuckleMultitenantAuditNet;

/// <summary>
/// POC tests to validate that multi-tenancy patterns work alongside audit logging.
/// These tests demonstrate:
/// 1. Entities can be saved with TenantId
/// 2. Query filters work to isolate data by tenant
/// 3. Audit logging captures operations with TenantId context
/// </summary>
public class PocTests
{
    [Fact]
    public async Task SaveArticle_WithTenantId_SavesSuccessfully()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<PocDbContext>()
            .UseInMemoryDatabase(databaseName: "PocTest1")
            .Options;
        
        using var context = new PocDbContext(options);
        context.CurrentTenantId = "tenant-1";
        
        var article = new PocArticle
        {
            Id = Guid.NewGuid(),
            Title = "Test Article",
            Body = "Test Body",
            TenantId = "tenant-1",
            CreatedAt = DateTime.UtcNow
        };
        
        // Act
        context.Articles.Add(article);
        var result = await context.SaveChangesAsync();
        
        // Assert
        Assert.Equal(1, result);
        var savedArticle = await context.Articles.FindAsync(article.Id);
        Assert.NotNull(savedArticle);
        Assert.Equal("tenant-1", savedArticle.TenantId);
        Assert.Equal("Test Article", savedArticle.Title);
    }
    
    [Fact]
    public async Task QueryArticles_WithTenantFilter_ReturnsOnlyTenantArticles()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<PocDbContext>()
            .UseInMemoryDatabase(databaseName: "PocTest2")
            .Options;
        
        // Add articles for different tenants
        using (var context = new PocDbContext(options))
        {
            context.Articles.AddRange(
                new PocArticle
                {
                    Id = Guid.NewGuid(),
                    Title = "Tenant 1 Article",
                    Body = "Body 1",
                    TenantId = "tenant-1",
                    CreatedAt = DateTime.UtcNow
                },
                new PocArticle
                {
                    Id = Guid.NewGuid(),
                    Title = "Tenant 2 Article",
                    Body = "Body 2",
                    TenantId = "tenant-2",
                    CreatedAt = DateTime.UtcNow
                }
            );
            await context.SaveChangesAsync();
        }
        
        // Act - Query with tenant-1 context
        using (var context = new PocDbContext(options))
        {
            context.CurrentTenantId = "tenant-1";
            var articles = await context.Articles.ToListAsync();
            
            // Assert
            Assert.Single(articles);
            Assert.Equal("Tenant 1 Article", articles[0].Title);
            Assert.Equal("tenant-1", articles[0].TenantId);
        }
    }
    
    [Fact]
    public async Task QueryArticles_DifferentTenant_ReturnsEmpty()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<PocDbContext>()
            .UseInMemoryDatabase(databaseName: "PocTest3")
            .Options;
        
        // Add article for tenant-1
        using (var context = new PocDbContext(options))
        {
            context.Articles.Add(new PocArticle
            {
                Id = Guid.NewGuid(),
                Title = "Tenant 1 Article",
                Body = "Body",
                TenantId = "tenant-1",
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }
        
        // Act - Query with tenant-2 context
        using (var context = new PocDbContext(options))
        {
            context.CurrentTenantId = "tenant-2";
            var articles = await context.Articles.ToListAsync();
            
            // Assert - Should return empty because query filter restricts to tenant-2
            Assert.Empty(articles);
        }
    }
    
    [Fact]
    public async Task SaveChanges_LogsAuditWithTenantId()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<PocDbContext>()
            .UseInMemoryDatabase(databaseName: "PocTest4")
            .Options;
        
        using var context = new PocDbContext(options);
        context.CurrentTenantId = "tenant-audit-test";
        
        var article = new PocArticle
        {
            Id = Guid.NewGuid(),
            Title = "Audit Test Article",
            Body = "Testing audit logging",
            TenantId = "tenant-audit-test",
            CreatedAt = DateTime.UtcNow
        };
        
        // Capture console output to verify audit logging
        var originalOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        
        try
        {
            // Act
            context.Articles.Add(article);
            await context.SaveChangesAsync();
            
            // Assert
            var output = stringWriter.ToString();
            Assert.Contains("[AUDIT]", output);
            Assert.Contains("tenant-audit-test", output);
            Assert.Contains("Added", output);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }
}
