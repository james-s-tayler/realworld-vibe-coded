## phase_13: Add Serilog Enricher for Tenant Context Logging

### Phase Overview

Configure Serilog with tenant context enricher to automatically include TenantId in all log entries. This enables filtering logs by tenant for debugging and observability. Combined with Audit.NET (configured in phase 4), provides comprehensive tenant-aware logging and auditing for production readiness.

**Scope Size:** Small (~5 steps)
**Risk Level:** Low (logging enrichment is additive)
**Estimated Complexity:** Low

### Prerequisites

What must be completed before starting this phase:
- Phase 12 completed (functional multi-tenancy tests passing)
- All tests passing
- Understanding of Serilog configuration from system-analysis.md

### Known Risks & Mitigations

**Risk 1:** Serilog enricher may not have access to tenant context
- **Likelihood:** Low
- **Impact:** Medium (TenantId not in logs)
- **Mitigation:** Use IMultiTenantContextAccessor in enricher to access current tenant
- **Fallback:** Add TenantId to log context manually in critical code paths

**Risk 2:** Performance impact of enricher on every log call
- **Likelihood:** Low
- **Impact:** Low (minor performance overhead)
- **Mitigation:** Serilog enrichers are designed to be lightweight. Tenant context is already resolved by Finbuckle.
- **Fallback:** Accept minor performance cost for valuable debugging capability

### Implementation Steps

**Part 1: Create Tenant Enricher**

1. **Create TenantEnricher class**
   - Create `App/Server/src/Server.Infrastructure/Logging/TenantEnricher.cs`
   - Implement `ILogEventEnricher` interface from Serilog
   - Inject IMultiTenantContextAccessor<AppTenantInfo> (use IHttpContextAccessor for access in enricher)
   - Expected outcome: Enricher class ready
   - Files affected: `App/Server/src/Server.Infrastructure/Logging/TenantEnricher.cs` (new)
   - Reality check: Code compiles

2. **Implement Enrich method**
   - In Enrich method, access current tenant via IMultiTenantContextAccessor
   - If TenantInfo is not null, add property: `logEvent.AddPropertyIfAbsent(new LogEventProperty("TenantId", new ScalarValue(tenantInfo.Id)))`
   - If TenantInfo is null (unauthenticated requests), skip or add null value
   - Expected outcome: TenantId added to log events
   - Files affected: `App/Server/src/Server.Infrastructure/Logging/TenantEnricher.cs`
   - Reality check: Enricher logic complete

**Part 2: Configure Serilog**

3. **Add enricher to Serilog configuration**
   - Open Serilog configuration in Program.cs or LoggerConfig.cs
   - Add enricher: `Log.Logger = new LoggerConfiguration().Enrich.With<TenantEnricher>()...`
   - Ensure TenantEnricher is registered in DI if needed (or use service provider in enricher)
   - Expected outcome: Enricher active
   - Files affected: `App/Server/src/Server.Web/Program.cs` or logging configuration
   - Reality check: Application starts without errors

**Part 3: Verify Logging**

4. **Test logging with tenant context**
   - Run application: `./build.sh RunLocalPublish`
   - Register and login as user
   - Perform actions that generate logs (create article, etc.)
   - Check logs in `Logs/Server.Web/Serilog/` directory
   - Verify TenantId property present in log entries
   - Expected outcome: Logs contain TenantId
   - Files affected: None (verification only)
   - Reality check: Grep logs for TenantId: `grep -r "TenantId" Logs/Server.Web/Serilog/`

5. **Verify Audit.NET logs also contain TenantId**
   - Check Audit.NET logs in `Logs/Server.Web/Audit.NET/`
   - Verify TenantId custom field is present (configured in phase 4)
   - Expected outcome: Audit logs contain TenantId
   - Files affected: None (verification only)
   - Reality check: Both Serilog and Audit.NET logs have TenantId

### Reality Testing During Phase

Test incrementally as you work:

```bash
# After enricher creation
./build.sh BuildServer

# After Serilog configuration
./build.sh RunLocalPublish
# Check logs

# Full validation
./build.sh TestServer
```

Don't wait until the end to test. Reality test after each major change.

### Expected Working State After Phase

When this phase is complete:
- Serilog configured with TenantEnricher
- TenantId automatically included in all log entries
- Logs can be filtered by tenant for debugging
- Audit.NET events include TenantId (from phase 4)
- Comprehensive tenant-aware logging and auditing
- System is production-ready with observability for multi-tenant scenarios
- **Migration complete** - all 13 phases finished successfully

### If Phase Fails

If this phase fails and cannot be completed:
1. Use mslearn MCP server to search for Serilog ILogEventEnricher examples
2. Check that IMultiTenantContextAccessor is accessible in enricher (may need IHttpContextAccessor)
3. Verify Serilog configuration is correct
4. Use debug-analysis.md for logging issues
5. If stuck, run `flowpilot stuck`

### Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh LintAllVerify
./build.sh BuildServer
./build.sh TestServer
./build.sh TestServerPostman
./build.sh TestE2e
```

All targets must pass. Migration is complete!

**Manual Verification Steps:**
1. Start application: `./build.sh RunLocalPublish`
2. Register and login
3. Create article, add comment
4. Check Serilog logs: `cat Logs/Server.Web/Serilog/*.log | grep TenantId`
5. Verify TenantId present in log entries
6. Check Audit.NET logs: `cat Logs/Server.Web/Audit.NET/*.json | grep TenantId`
7. Verify TenantId in audit events
8. Test filtering logs by TenantId for debugging

**Final Migration Validation:**
1. All 13 phases completed successfully
2. All tests passing (45 functional, 51+ E2E, 5 Postman)
3. Multi-tenancy fully implemented with Finbuckle.MultiTenant
4. Data isolation enforced on reads (query filters) and writes (TenantId assignment)
5. Comprehensive logging and auditing with tenant context
6. System ready for production use as internal company social network
