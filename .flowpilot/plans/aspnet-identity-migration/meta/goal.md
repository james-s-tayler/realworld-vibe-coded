## Goal

Migrate the authentication/authorization system to ASP.NET Identity.

## Requirements
- **No UI Dependency**: The migration does not require using any UI components provided by ASP.NET Identity.
- **Endpoint Switch**: The application must transition to using endpoints provided by ASP.NET Identity, and not try to create wrappers or use the existing endpoints. We don't need to maintain compatibility with the Realworld spec or with any existing data since the application is in development phase and starts from a blank database.
- **Testing Parity**: Maintain the same level of testing (Integration tests, Postman tests, E2E Playwright tests). It's acceptable to revise tests so they match ASP.NET Identity’s workflows, rather than forcing ASP.NET Identity to conform to the current tests’ structure.
- **Audit.NET Compatibility**: The project uses Audit.NET, which subclasses `DbContext`. Since ASP.NET Identity also involves `DbContext` subclassing, cross-compatibility must be investigated. Audit.NET is believed to have support for ASP.NET Identity, but confirmation and integration details should be researched and documented (consider a web search or check the Audit.NET docs).
- **Cookie based auth**: we must use cookie based authentication and not JWT.