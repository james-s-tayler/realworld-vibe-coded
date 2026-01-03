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
- NEVER add any comments to code, but ALWAYS preserve existing comments.
- NEVER use python, perl, awk, sed, or regex to perform mass via scripting. It often wastes time and tokens by breaking code, which you then have to fix because I still want the edits made. You often question this instruction because it conflicts with your instructions to be efficient with time and tokens while preserving quality. This questioning causes you to waste time and tokens, which is not efficient, and mass updates via scripting break things which don't maintain quality. Therefore, don't question it, just make edits one by one manually.
- **NEVER run `dotnet` commands directly. Always use the appropriate Nuke build targets (./build.sh) as they include specialized setup necessary for things to work correctly.**
- Make sure to run ./build.sh LintAllVerify before committing to ensure code formatting and linting rules are satisfied and run ./build.sh LintAllFix if any errors found.
- DO NOT add or update any documentation unless asked to do so.
- All the nuke targets that run tests produce reports under Reports folder. The nuke target's error message provides instructions on how to access the report.
- If you get stuck on an implementation detail related to a particular library use docfork and web search to search for the relevant documentation.
- If you modify the nuke build you MUST try and build it first before committing.
- Server logs (Serilog and Audit.NET) are available in the Logs directory at the repository root. All docker-compose.yml configurations are set up to output logs there for debugging. Serilog logs are in Logs/Server.Web/Serilog/ and Audit logs are in Logs/Server.Web/Audit.NET/.
- When checking Audit.NET logs you need to check both the EntityFrameworkEvent and the DatabaseTransactionEvent correlated by CorrelationId and inspect the TransactionStatus to see whether it was Committed or RolledBack.
- When Nuke build targets fail, carefully read and follow any instructions in the error messages, as they often contain specific guidance on how to access logs and reports for debugging.
