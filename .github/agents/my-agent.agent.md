---
# Fill in the fields below to create a basic custom agent for your repository.
# The Copilot CLI can be used for local testing: https://gh.io/customagents/cli
# To make this agent available, merge this file into the default repository branch.
# For format details, see: https://gh.io/customagents/config

name: standard
description: default do's and dont's included in instructions
---

# My Agent

You help implement requests in the codebase, subject to the following conditions:

- You are not permitted to suppress warnings or errors in code unless explicitly instructed to do so.
- You are not permitted to modify any Analyzers unless explicitly instructed to do so.
- Don't hardcode things, do dummy/toy implementations or use magic strings.
- Do not use python, perl, awk, sed, or regex to perform mass updates as it often results in breaking the code. Only do direct updates with the edit tool.
- Make sure to run ./build.sh LintAllVerify before committing to ensure code formatting and linting rules are satisfied and run ./build.sh LintAllFix if any errors found.
- Make sure the postman tests are passing before finishing by running nuke TestServerPostman.
- DO NOT add or update any documentation unless asked to do so.
- All the nuke targets that run tests produce reports under Reports folder. Make sure to check them if any test fails.
- If you get stuck on an implementation detail related to a particular library use the docs-mcp-server to search for the relevant documentation.
- If you modify the nuke build you MUST try and build it first before committing.
- Server logs (Serilog and Audit.NET) are available in the Logs directory at the repository root. All docker-compose.yml configurations are set up to output logs there for debugging. Serilog logs are in Logs/Server.Web/Serilog/ and Audit logs are in Logs/Server.Web/Audit.NET/.
- When checking Audit.NET logs you need to check both the EntityFrameworkEvent and the DatabaseTransactionEvent correlated by CorrelationId and inspect the TransactionStatus to see whether it was Committed or RolledBack.
- When Nuke build targets fail, carefully read and follow any instructions in the error messages, as they often contain specific guidance on how to access logs and reports for debugging.
