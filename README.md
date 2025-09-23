# realworld-vibe-coded
An attempt at implementing gothinkster/realworld purely through vibe-coding

## Specifications
- [Endpoints](https://docs.realworld.show/specifications/backend/endpoints/)

## API E2E tests (Postman/Newman)
This repo includes a minimal Postman/Newman harness to smoke-test the API locally and in CI.

Prerequisites:
- Docker (for the Newman runner)
- .NET SDK (for running the API)
- A developer HTTPS certificate installed for ASP.NET Core (if missing, run `dotnet dev-certs https`)

Quickstart:
- Run the collection (resets the local SQLite DB, starts API on https://localhost:5041, runs Newman, writes JUnit report):

```
make postman/run
```

Notes:
- Results are saved to `e2e/postman/reports/newman.junit.xml`.
- You can override defaults:
  - `POSTMAN_PORT` (default 5041)
  - `POSTMAN_BASE_URL` (default https://localhost:5041)
- The Docker runner maps `host.docker.internal` to the host, so the Newman container can call the host API.

## Notes on Vibe Coding
- [Copilot Docs](https://code.visualstudio.com/docs/copilot/overview)
- [Awesome Copilot](https://github.com/github/awesome-copilot/tree/main)
- [Use prompt files in VS Code](https://code.visualstudio.com/docs/copilot/customization/prompt-files)
- [Adding repository custom instructions for GitHub Copilot](https://docs.github.com/en/copilot/how-tos/configure-custom-instructions/add-repository-instructions?tool=jetbrains)

## Notes on MCP
- [MCP Servers](https://github.com/modelcontextprotocol/servers/tree/main)
- [GitHub MCP Server](https://github.com/github/github-mcp-server)
- [Hyperbrowser](https://github.com/hyperbrowserai/mcp)
- [Kintone](https://github.com/kintone/mcp-server?tab=readme-ov-file)
- [Microsoft Learn MCP Server](https://github.com/microsoftdocs/mcp)
- [Playwright MCP](https://github.com/microsoft/playwright-mcp)
- [Postman MCP Server](https://github.com/postmanlabs/postman-mcp-server)
- [Azure MCP](https://github.com/Azure-Samples/mcp)
- [MCP Devcontainers](https://github.com/AI-QL/mcp-devcontainers)
- [Excel MCP Server](https://github.com/haris-musa/excel-mcp-server)
- [Excel to JSON](https://github.com/he-yang/excel-to-json-mcp)
- [Figma MCP Server](https://github.com/paulvandermeijs/figma-mcp)
- [n8n MCP Server](https://github.com/leonardsellem/n8n-mcp-server)
- [Unleash Feature Toggle MCP](https://github.com/cuongtl1992/unleash-mcp)
- [Workflowy](https://github.com/danield137/mcp-workflowy)