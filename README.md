# realworld-vibe-coded
An attempt at implementing gothinkster/realworld purely through vibe-coding

## Specifications
- [Endpoints](https://docs.realworld.show/specifications/backend/endpoints/)

## Notes on Vibe Coding
- [Copilot Docs](https://code.visualstudio.com/docs/copilot/overview)
- [Awesome Copilot](https://github.com/github/awesome-copilot/tree/main)
- [Use prompt files in VS Code](https://code.visualstudio.com/docs/copilot/customization/prompt-files)
- [Adding repository custom instructions for GitHub Copilot](https://docs.github.com/en/copilot/how-tos/configure-custom-instructions/add-repository-instructions?tool=jetbrains)

## Notes on MCP
- [Docs MCP Server](https://github.com/arabold/docs-mcp-server)
  - install via `sudo npm install -g @arabold/docs-mcp-server@latest`
  - run via `npx @arabold/docs-mcp-server@latest`
  - Add MCP server settings
  - browse to [http://localhost:6280](http://localhost:6280) and slurp up the docs

Inside Github Copilot in the repo settings for copilot

    { 
      "mcpServers": {
        "docs-mcp-server": {
          "type": "sse",
          "url": "http://localhost:6280/sse",
          "tools": ["SearchTool"]
        }
      }
    }

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

## Code Quality

This repository enforces strict code quality standards:

### Warnings as Errors
All C# projects are configured to treat warnings as errors via a root-level `Directory.Build.props` file. This ensures that:
- No code is committed with compiler warnings
- Code quality standards are consistently enforced
- Build failures occur immediately when warnings are introduced

### Whitespace Linting
The repository enforces strict whitespace rules via `.editorconfig`:
- **Trailing whitespace**: Not allowed
- **Final newline**: Required
- **Line endings**: CRLF (Windows-style)
- **Character encoding**: UTF-8 with BOM for C# files
- **Formatting**: IDE0055 diagnostic is set to error severity

### Linting Commands
Use these Nuke build targets to verify and fix code formatting:

```bash
# Verify all C# code formatting (fails on violations)
./build.sh LintAllVerify

# Automatically fix all C# code formatting issues
./build.sh LintAllFix

# Verify/fix specific areas
./build.sh LintServerVerify   # Backend only
./build.sh LintServerFix       # Backend only
./build.sh LintNukeVerify      # Build scripts only
./build.sh LintNukeFix         # Build scripts only
./build.sh LintClientVerify    # Frontend only
./build.sh LintClientFix       # Frontend only
```

**Before committing**: Always run `./build.sh LintAllVerify` to ensure your changes meet code quality standards.
