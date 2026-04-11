---
paths:
  - Task/Runner/**
---

## Nuke Build Conventions

### Worktree Port Isolation

Every listening service in `RunLocal*` targets MUST use `Constants.Worktree.GetPortOffset(RootDirectory)` for its port. This includes Vite, backend, MCP servers — any service that binds a port.

### Vite Environment Variables

Never pass `VITE_`-prefixed env vars from Nuke unless intended for browser-side use. Vite auto-exposes `VITE_*` to client code via `import.meta.env`. Use unprefixed names (e.g., `API_PROXY_TARGET`). `VITE_DEV_PORT` is the exception — it's consumed by vite.config.ts server-side via `process.env`, not `import.meta.env`.
