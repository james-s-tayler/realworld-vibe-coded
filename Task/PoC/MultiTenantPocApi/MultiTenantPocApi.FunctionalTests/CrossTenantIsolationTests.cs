using MultiTenantPocApi.Endpoints;

namespace MultiTenantPocApi.FunctionalTests;

/// <summary>
/// Tests verifying cross-tenant isolation - the core multi-tenancy requirement
/// These tests prove that data created in one tenant is NOT visible in another tenant
/// </summary>
[Collection("POC Sequential Tests")]
public class CrossTenantIsolationTests : IClassFixture<PocApiFixture>
{
    private readonly PocApiFixture _fixture;

    public CrossTenantIsolationTests(PocApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateArticle_InTenant1_NotVisibleInTenant2()
    {
        // Arrange
        
        var tenant1Client = _fixture.CreateTenantClient("tenant-1");
        var tenant2Client = _fixture.CreateTenantClient("tenant-2");

        var createRequest = new CreateArticleRequest
        {
            Title = "Tenant 1 Article",
            Body = "This article belongs to tenant-1"
        };

        // Act - Create article in tenant-1
        var createResponse = await tenant1Client.PostAsJsonAsync("/api/articles", createRequest);
        createResponse.EnsureSuccessStatusCode();
        
        var createdArticle = await createResponse.Content.ReadFromJsonAsync<ArticleResponse>();
        createdArticle.ShouldNotBeNull();
        createdArticle.TenantId.ShouldBe("tenant-1");

        // Act - Query articles in tenant-2
        var listResponse = await tenant2Client.GetAsync("/api/articles");
        listResponse.EnsureSuccessStatusCode();
        
        var articlesInTenant2 = await listResponse.Content.ReadFromJsonAsync<ArticlesListResponse>();

        // Assert - Tenant-2 should NOT see tenant-1's article
        articlesInTenant2.ShouldNotBeNull();
        articlesInTenant2.ArticlesCount.ShouldBe(0);
        articlesInTenant2.Articles.ShouldBeEmpty();
    }

    [Fact]
    public async Task CreateArticle_InTenant2_NotVisibleInTenant1()
    {
        // Arrange
        
        var tenant1Client = _fixture.CreateTenantClient("tenant-1");
        var tenant2Client = _fixture.CreateTenantClient("tenant-2");

        var createRequest = new CreateArticleRequest
        {
            Title = "Tenant 2 Article",
            Body = "This article belongs to tenant-2"
        };

        // Act - Create article in tenant-2
        var createResponse = await tenant2Client.PostAsJsonAsync("/api/articles", createRequest);
        createResponse.EnsureSuccessStatusCode();
        
        var createdArticle = await createResponse.Content.ReadFromJsonAsync<ArticleResponse>();
        createdArticle.ShouldNotBeNull();
        createdArticle.TenantId.ShouldBe("tenant-2");

        // Act - Query articles in tenant-1
        var listResponse = await tenant1Client.GetAsync("/api/articles");
        listResponse.EnsureSuccessStatusCode();
        
        var articlesInTenant1 = await listResponse.Content.ReadFromJsonAsync<ArticlesListResponse>();

        // Assert - Tenant-1 should NOT see tenant-2's article
        articlesInTenant1.ShouldNotBeNull();
        articlesInTenant1.ArticlesCount.ShouldBe(0);
        articlesInTenant1.Articles.ShouldBeEmpty();
    }

    [Fact]
    public async Task CreateArticles_InBothTenants_EachTenantSeesOnlyTheirOwn()
    {
        // Arrange
        
        var tenant1Client = _fixture.CreateTenantClient("tenant-1");
        var tenant2Client = _fixture.CreateTenantClient("tenant-2");

        // Act - Create 2 articles in tenant-1
        var tenant1Request1 = new CreateArticleRequest
        {
            Title = "Tenant 1 Article 1",
            Body = "First article in tenant-1"
        };
        var tenant1Request2 = new CreateArticleRequest
        {
            Title = "Tenant 1 Article 2",
            Body = "Second article in tenant-1"
        };

        await tenant1Client.PostAsJsonAsync("/api/articles", tenant1Request1);
        await tenant1Client.PostAsJsonAsync("/api/articles", tenant1Request2);

        // Act - Create 3 articles in tenant-2
        var tenant2Request1 = new CreateArticleRequest
        {
            Title = "Tenant 2 Article 1",
            Body = "First article in tenant-2"
        };
        var tenant2Request2 = new CreateArticleRequest
        {
            Title = "Tenant 2 Article 2",
            Body = "Second article in tenant-2"
        };
        var tenant2Request3 = new CreateArticleRequest
        {
            Title = "Tenant 2 Article 3",
            Body = "Third article in tenant-2"
        };

        await tenant2Client.PostAsJsonAsync("/api/articles", tenant2Request1);
        await tenant2Client.PostAsJsonAsync("/api/articles", tenant2Request2);
        await tenant2Client.PostAsJsonAsync("/api/articles", tenant2Request3);

        // Act - Query articles in each tenant
        var tenant1Response = await tenant1Client.GetAsync("/api/articles");
        var tenant2Response = await tenant2Client.GetAsync("/api/articles");

        var tenant1Articles = await tenant1Response.Content.ReadFromJsonAsync<ArticlesListResponse>();
        var tenant2Articles = await tenant2Response.Content.ReadFromJsonAsync<ArticlesListResponse>();

        // Assert - Tenant-1 sees only 2 articles
        tenant1Articles.ShouldNotBeNull();
        tenant1Articles.ArticlesCount.ShouldBe(2);
        tenant1Articles.Articles.ShouldAllBe(a => a.TenantId == "tenant-1");
        tenant1Articles.Articles.ShouldAllBe(a => a.Title.StartsWith("Tenant 1"));

        // Assert - Tenant-2 sees only 3 articles
        tenant2Articles.ShouldNotBeNull();
        tenant2Articles.ArticlesCount.ShouldBe(3);
        tenant2Articles.Articles.ShouldAllBe(a => a.TenantId == "tenant-2");
        tenant2Articles.Articles.ShouldAllBe(a => a.Title.StartsWith("Tenant 2"));
    }

    [Fact]
    public async Task CreateArticle_WithoutTenantHeader_Returns400OrEmptyResult()
    {
        // Arrange
        
        var clientWithoutTenant = _fixture.Client; // No X-Tenant-Id header

        var createRequest = new CreateArticleRequest
        {
            Title = "Article Without Tenant",
            Body = "This should fail or have no tenant"
        };

        // Act - Attempt to create article without tenant context
        var createResponse = await clientWithoutTenant.PostAsJsonAsync("/api/articles", createRequest);

        // Assert - Should either fail or create with null tenant (depending on Finbuckle config)
        // For POC, we allow this to proceed but TenantId will be "unknown"
        if (createResponse.IsSuccessStatusCode)
        {
            var createdArticle = await createResponse.Content.ReadFromJsonAsync<ArticleResponse>();
            createdArticle.ShouldNotBeNull();
            // TenantId should be unknown or empty when no tenant context
            (createdArticle.TenantId == "unknown" || string.IsNullOrEmpty(createdArticle.TenantId)).ShouldBeTrue();
        }
    }

    [Fact]
    public async Task QueryArticles_SameTenantMultipleTimes_ReturnsConsistentResults()
    {
        // Arrange
        
        var tenant1Client = _fixture.CreateTenantClient("tenant-1");

        var createRequest = new CreateArticleRequest
        {
            Title = "Consistent Test Article",
            Body = "Testing consistency"
        };

        await tenant1Client.PostAsJsonAsync("/api/articles", createRequest);

        // Act - Query multiple times
        var response1 = await tenant1Client.GetAsync("/api/articles");
        var response2 = await tenant1Client.GetAsync("/api/articles");
        var response3 = await tenant1Client.GetAsync("/api/articles");

        var articles1 = await response1.Content.ReadFromJsonAsync<ArticlesListResponse>();
        var articles2 = await response2.Content.ReadFromJsonAsync<ArticlesListResponse>();
        var articles3 = await response3.Content.ReadFromJsonAsync<ArticlesListResponse>();

        // Assert - All queries return same results
        articles1.ShouldNotBeNull();
        articles2.ShouldNotBeNull();
        articles3.ShouldNotBeNull();

        articles1.ArticlesCount.ShouldBe(1);
        articles2.ArticlesCount.ShouldBe(1);
        articles3.ArticlesCount.ShouldBe(1);

        articles1.Articles[0].Id.ShouldBe(articles2.Articles[0].Id);
        articles2.Articles[0].Id.ShouldBe(articles3.Articles[0].Id);
    }
}
