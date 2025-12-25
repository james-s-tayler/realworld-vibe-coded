## phase_9: Update Handlers to Set TenantId When Creating Entities

### Phase Overview

Update CreateArticleHandler, CreateCommentHandler, and Tag creation logic to set TenantId from current tenant context when creating new entities. This ensures all new entities are associated with the current organization. Data isolation now enforced on both reads (query filters) and writes (explicit TenantId assignment).

**Scope Size:** Small (~6 steps)
**Risk Level:** Low (straightforward context propagation)
**Estimated Complexity:** Low

### Prerequisites

What must be completed before starting this phase:
- Phase 8 completed (tenant resolution working)
- IMultiTenantContextAccessor available and returning tenant context
- All tests passing with tenant resolution

### Known Risks & Mitigations

**Risk 1:** Handlers may not have access to IMultiTenantContextAccessor
- **Likelihood:** Low
- **Impact:** Medium (can't set TenantId)
- **Mitigation:** Inject IMultiTenantContextAccessor<AppTenantInfo> into handlers
- **Fallback:** Create service to wrap tenant access if injection doesn't work

**Risk 2:** TenantInfo may be null in some contexts
- **Likelihood:** Low (authentication required from phase 3)
- **Impact:** Medium (exception thrown)
- **Mitigation:** All create endpoints require authentication, so tenant should always be resolved. Add null check anyway.
- **Fallback:** Throw meaningful exception if tenant not resolved

### Implementation Steps

**Part 1: Update CreateArticleHandler**

1. **Inject IMultiTenantContextAccessor into CreateArticleHandler**
   - Open `App/Server/src/Server.UseCases/Articles/CreateArticleHandler.cs`
   - Add constructor parameter: `IMultiTenantContextAccessor<AppTenantInfo>`
   - Expected outcome: Handler has access to current tenant
   - Files affected: `App/Server/src/Server.UseCases/Articles/CreateArticleHandler.cs`
   - Reality check: Code compiles

2. **Set Article.TenantId from tenant context**
   - In Handle method, before saving Article:
   - `article.TenantId = _tenantAccessor.TenantInfo.Id;`
   - Add null check: throw exception if TenantInfo is null
   - Expected outcome: Article created with TenantId
   - Files affected: `App/Server/src/Server.UseCases/Articles/CreateArticleHandler.cs`
   - Reality check: Article creation sets TenantId

**Part 2: Update CreateCommentHandler**

3. **Inject IMultiTenantContextAccessor into CreateCommentHandler**
   - Open `App/Server/src/Server.UseCases/Comments/CreateCommentHandler.cs`
   - Add constructor parameter: `IMultiTenantContextAccessor<AppTenantInfo>`
   - Expected outcome: Handler has access to current tenant
   - Files affected: `App/Server/src/Server.UseCases/Comments/CreateCommentHandler.cs`
   - Reality check: Code compiles

4. **Set Comment.TenantId from tenant context**
   - In Handle method, before saving Comment:
   - `comment.TenantId = _tenantAccessor.TenantInfo.Id;`
   - Add null check
   - Expected outcome: Comment created with TenantId
   - Files affected: `App/Server/src/Server.UseCases/Comments/CreateCommentHandler.cs`
   - Reality check: Comment creation sets TenantId

**Part 3: Update Tag Creation**

5. **Find Tag creation logic**
   - Tags may be created within CreateArticleHandler or separate handler
   - Use RoslynMCP FindUsages to find where `new Tag()` is called
   - Set Tag.TenantId when created: `tag.TenantId = _tenantAccessor.TenantInfo.Id;`
   - Expected outcome: Tags created with TenantId
   - Files affected: Handler that creates Tags
   - Reality check: Tag creation sets TenantId

**Part 4: Update Tests**

6. **Update handler tests to verify TenantId set**
   - Update tests for CreateArticleHandler, CreateCommentHandler
   - Verify created entities have TenantId matching test tenant
   - Expected outcome: Tests validate TenantId assignment
   - Files affected: `App/Server/tests/Server.FunctionalTests/Articles/*Tests.cs`, `App/Server/tests/Server.FunctionalTests/Comments/*Tests.cs`
   - Reality check: Tests pass, verify TenantId set correctly

### Reality Testing During Phase

Test incrementally as you work:

```bash
# After each handler update
./build.sh LintServerVerify
./build.sh BuildServer

# After all handler updates
./build.sh TestServer

# Full validation (run individual Postman targets)
./build.sh TestServerPostmanArticlesEmpty
./build.sh TestServerPostmanAuth
./build.sh TestServerPostmanProfiles
./build.sh TestServerPostmanFeedAndArticles
./build.sh TestServerPostmanArticle
./build.sh TestE2e
```

Don't wait until the end to test. Reality test after each major change.

### Expected Working State After Phase

When this phase is complete:
- CreateArticleHandler sets Article.TenantId from tenant context
- CreateCommentHandler sets Comment.TenantId from tenant context
- Tag creation sets Tag.TenantId from tenant context
- All new entities associated with current organization
- Data isolation enforced on both reads (query filters) and writes (TenantId assignment)
- System is fully multi-tenant with complete CRUD isolation
- Tests verify TenantId set correctly on created entities
- Ready for comprehensive multi-tenancy testing in phases 10-12

### If Phase Fails

If this phase fails and cannot be completed:
1. Verify IMultiTenantContextAccessor is registered in DI (should be automatic with Finbuckle)
2. Check that TenantInfo is not null in handlers (all create endpoints require auth)
3. Use RoslynMCP FindUsages to find all entity creation code
4. Use debug-analysis.md if TenantId not being set correctly
5. If stuck, run `flowpilot stuck`

### Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh LintServerVerify
./build.sh BuildServer
./build.sh TestServer
./build.sh TestServerPostmanArticlesEmpty
./build.sh TestServerPostmanAuth
./build.sh TestServerPostmanProfiles
./build.sh TestServerPostmanFeedAndArticles
./build.sh TestServerPostmanArticle
./build.sh TestE2e
```

All targets must pass before proceeding to the next phase.

**Manual Verification Steps:**
1. Start application: `./build.sh RunLocalPublish`
2. Register and login as User1
3. Create article - check database, verify Article.TenantId matches User1's Organization.Id
4. Create comment - check database, verify Comment.TenantId matches User1's Organization.Id
5. Register and login as User2 (different organization)
6. Verify User2 cannot see User1's articles (query filters work)
7. Verify User2 can create their own articles with User2's Organization.TenantId
