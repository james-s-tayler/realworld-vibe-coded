using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinbuckleMultitenantAuditNet;

/// <summary>
/// POC tests to validate that MultiTenantIdentityDbContext works alongside audit logging.
/// These tests demonstrate:
/// 1. Entities can be saved with TenantId using Finbuckle's automatic association
/// 2. Query filters work to isolate data by tenant automatically via Finbuckle
/// 3. Audit logging captures operations with TenantId context from TenantInfo
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
        
        var tenant = new PocTenantInfo 
        { 
            Id = "tenant-1", 
            Identifier = "tenant-1", 
            Name = "Tenant One" 
        };
        
        // Use Finbuckle's factory method to create context with tenant
        using var context = MultiTenantDbContext.Create(tenant, options);
        
        var article = new PocArticle
        {
            Id = Guid.NewGuid(),
            Title = "Test Article",
            Body = "Test Body",
            CreatedAt = DateTime.UtcNow
            // Note: TenantId is automatically set by Finbuckle on SaveChanges
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
        
        var tenant1 = new PocTenantInfo 
        { 
            Id = "tenant-1", 
            Identifier = "tenant-1", 
            Name = "Tenant One" 
        };
        
        var tenant2 = new PocTenantInfo 
        { 
            Id = "tenant-2", 
            Identifier = "tenant-2", 
            Name = "Tenant Two" 
        };
        
        // Add articles for different tenants using Finbuckle's factory
        using (var context = MultiTenantDbContext.Create(tenant1, options))
        {
            context.Articles.Add(new PocArticle
            {
                Id = Guid.NewGuid(),
                Title = "Tenant 1 Article",
                Body = "Body 1",
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }
        
        using (var context = MultiTenantDbContext.Create(tenant2, options))
        {
            context.Articles.Add(new PocArticle
            {
                Id = Guid.NewGuid(),
                Title = "Tenant 2 Article",
                Body = "Body 2",
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }
        
        // Act - Query with tenant-1 context
        using (var context = MultiTenantDbContext.Create(tenant1, options))
        {
            var articles = await context.Articles.ToListAsync();
            
            // Assert - Finbuckle's automatic query filter restricts to tenant-1
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
        
        var tenant1 = new PocTenantInfo 
        { 
            Id = "tenant-1", 
            Identifier = "tenant-1", 
            Name = "Tenant One" 
        };
        
        var tenant2 = new PocTenantInfo 
        { 
            Id = "tenant-2", 
            Identifier = "tenant-2", 
            Name = "Tenant Two" 
        };
        
        // Add article for tenant-1
        using (var context = MultiTenantDbContext.Create(tenant1, options))
        {
            context.Articles.Add(new PocArticle
            {
                Id = Guid.NewGuid(),
                Title = "Tenant 1 Article",
                Body = "Body",
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }
        
        // Act - Query with tenant-2 context
        using (var context = MultiTenantDbContext.Create(tenant2, options))
        {
            var articles = await context.Articles.ToListAsync();
            
            // Assert - Finbuckle's query filter automatically restricts to tenant-2, so result is empty
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
        
        var tenant = new PocTenantInfo 
        { 
            Id = "tenant-audit-test", 
            Identifier = "tenant-audit-test", 
            Name = "Audit Test Tenant" 
        };
        
        using var context = MultiTenantDbContext.Create(tenant, options);
        
        var article = new PocArticle
        {
            Id = Guid.NewGuid(),
            Title = "Audit Test Article",
            Body = "Testing audit logging",
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
            
            // Assert - Audit logging captures TenantId from TenantInfo
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
