# Documentation Index

This directory contains project documentation and planning documents.

## Planning Documents

### [Frontend Replication Plan](./FRONTEND_REPLICATION_PLAN.md)
**Status:** ✅ Complete  
**Purpose:** Comprehensive plan to replicate the RealWorld frontend using Carbon Design System

**Contents:**
- Current state assessment of existing implementation
- Complete UI specification for all RealWorld pages
- Component mapping from RealWorld to Carbon Design System
- Feature parity matrix for all screens and functionality
- Detailed implementation roadmap with 4 phases
- File structure recommendations
- Estimated timeline: 77-105 developer hours

**Key Sections:**
- Required Pages/Routes (9 pages)
- Component Mapping (Layout, Forms, Display, Navigation, Interactive)
- Feature Parity Matrix (Authentication, Articles, Comments, Profiles, Tags)
- Implementation Roadmap (Phase 1-4 with priorities)

### [Playwright Demo Access Issue](./PLAYWRIGHT_DEMO_ACCESS_ISSUE.md)
**Status:** ✅ Documented  
**Purpose:** Explains why RealWorld demo sites are blocked via Playwright MCP server

**Contents:**
- Error details and root cause analysis
- Explanation of ERR_BLOCKED_BY_CLIENT
- Why it happens in GitHub Actions runner environment
- Alternative research methods used
- Impact assessment (no impact on planning)
- Recommendations for local development and testing

**Key Finding:** Environment-level domain blocking for security; alternative research methods successfully gathered all needed information.

## Technical Documentation

### [CI Documentation](./ci.md)
GitHub Actions CI/CD pipeline documentation

### [Audit Documentation](./AUDIT.md)
System audit and security documentation

### [Playwright Docker Optimization](./PLAYWRIGHT_DOCKER_OPTIMIZATION.md)
Performance optimization notes for Playwright in Docker

---

## Quick Navigation

**Want to implement the frontend?**  
→ Start with [Frontend Replication Plan](./FRONTEND_REPLICATION_PLAN.md)

**Curious about the Playwright blocking issue?**  
→ Read [Playwright Demo Access Issue](./PLAYWRIGHT_DEMO_ACCESS_ISSUE.md)

**Working on CI/CD?**  
→ Check [CI Documentation](./ci.md)

---

**Last Updated:** 2025-11-10
