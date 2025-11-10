# Playwright MCP Server Demo Access Issue

## Issue Summary

When attempting to access RealWorld demo sites via the Playwright MCP server, navigation fails with `ERR_BLOCKED_BY_CLIENT` errors.

## Error Details

```
Error: page.goto: net::ERR_BLOCKED_BY_CLIENT at https://demo.realworld.show/#/
Call log:
  - navigating to "https://demo.realworld.show/#/", waiting until "domcontentloaded"
```

Similar error occurs for:
- `https://demo.realworld.show/#/`
- `https://angular.realworld.io/`

## Root Cause Analysis

### What is ERR_BLOCKED_BY_CLIENT?

`ERR_BLOCKED_BY_CLIENT` is a Chromium browser error that indicates a web resource was blocked by client-side software or policies. This is **not a server-side issue** but rather a restriction on the client (browser) side.

### Common Causes

1. **Browser Extensions:** Ad blockers, privacy extensions, or content filters
2. **Security Software:** Antivirus or firewall software with web protection
3. **Environment Policies:** Network-level filtering or domain blocking
4. **Chromium-Specific Issues:** Some routing or proxy configurations can trigger false positives

### Why It's Happening in This Environment

In the GitHub Actions runner environment (or similar sandboxed environment), the most likely cause is:

**Network-Level Domain Filtering:**
- The infrastructure may have allowlist/blocklist policies for external domains
- Security policies may block certain categories of websites
- Content delivery networks or demo sites may be restricted by default

This is a security feature, not a bug. GitHub Actions runners have limited internet access with blocked domains to:
- Prevent data exfiltration
- Reduce attack surface
- Control outbound traffic for security/compliance

### Evidence

1. **Consistent Blocking:** Both demo.realworld.show and angular.realworld.io are blocked
2. **Clean Browser Context:** Playwright runs in a clean context without extensions
3. **Environment Restrictions:** As noted in the instructions: "You have limited access to the internet, but many domains are blocked"

## Workarounds Attempted

### ❌ Direct Navigation
```typescript
playwright-browser_navigate("https://demo.realworld.show/#/")
// Result: ERR_BLOCKED_BY_CLIENT
```

### ❌ Alternative Demo Sites
```typescript
playwright-browser_navigate("https://angular.realworld.io/")
// Result: ERR_BLOCKED_BY_CLIENT
```

## Alternative Research Methods Used

Since direct access to the demo sites was blocked, we used these alternative approaches:

### ✅ 1. Web Search via MCP Server
Used `github-mcp-server-web_search` to gather information about:
- RealWorld UI component structure
- Page layouts and user flows
- API specifications and endpoints
- Carbon Design System component mapping

### ✅ 2. Documentation Review
Reviewed official documentation:
- [RealWorld GitHub Repository](https://github.com/gothinkster/realworld)
- [RealWorld API Spec](https://docs.realworld.show/specifications/backend/endpoints/)
- [Carbon Design System Documentation](https://carbondesignsystem.com/)
- Existing backend API implementation in this repository

### ✅ 3. Code Analysis
Analyzed existing codebase:
- Backend API endpoints in `App/Server/src/Server.Web/`
- Partial frontend implementation in `App/Client/src/`
- API types and interfaces
- Authentication flow

### ✅ 4. Specification Study
The RealWorld specification is comprehensive and standardized:
- Clear endpoint definitions
- Documented request/response formats
- Known UI patterns (Medium.com clone)
- Multiple reference implementations available

## Impact Assessment

### No Impact on Planning
Despite not being able to interact with the live demo via Playwright:
- ✅ Complete API specification is well-documented
- ✅ UI patterns are standardized and known
- ✅ Backend implementation is complete and testable
- ✅ Carbon Design System provides all needed components
- ✅ Multiple reference implementations exist for comparison

### What We Couldn't Do
- Record actual network requests from the demo site
- Take screenshots of the live UI for exact visual reference
- Interact with the demo to discover edge cases
- Measure performance characteristics

### What We Did Instead
- Documented all known UI components from specification
- Mapped components to Carbon Design System equivalents
- Created comprehensive implementation roadmap
- Reviewed backend API to understand data structures
- Analyzed existing frontend code for patterns

## Recommendations

### For Local Development
If developers need to reference the live demo:
1. **Access from local machine:** Browse to https://demo.realworld.show in regular browser
2. **Use browser DevTools:** Inspect network requests, DOM structure, CSS
3. **Take screenshots:** Document UI patterns for reference
4. **Test API calls:** Use Postman or curl to understand API behavior

### For Automated Testing
If we need to test against a live RealWorld instance:
1. **Deploy our own backend:** Backend is complete and can be hosted
2. **Use local instance:** Run backend locally for E2E tests
3. **Mock API responses:** Use MSW or similar for frontend tests
4. **Reference implementations:** Compare with official React/Angular implementations

### For Future Playwright Usage
If domain access is needed:
1. **Request domain allowlisting:** Contact infrastructure team to allow specific domains
2. **Use proxy:** Configure a proxy that's on the allowlist
3. **Deploy own instance:** Host the demo app on an allowed domain
4. **Alternative tools:** Use other MCP servers that don't require browser access

## Conclusion

While the `ERR_BLOCKED_BY_CLIENT` error prevented direct Playwright interaction with RealWorld demo sites, it did not impact our ability to create a comprehensive replication plan. The RealWorld specification is well-documented, the backend is complete, and we have all the information needed to implement the frontend with Carbon Design System.

The blocking is a security feature of the environment, not a bug or issue with Playwright or the demo sites themselves. For reference needs, developers should access the demo from their local machines or deploy their own instance.

---

**Related Documents:**
- [Frontend Replication Plan](./FRONTEND_REPLICATION_PLAN.md)
- [Playwright Docker Optimization](./PLAYWRIGHT_DOCKER_OPTIMIZATION.md)

**Date:** 2025-11-10  
**Status:** Documented - No Action Required
