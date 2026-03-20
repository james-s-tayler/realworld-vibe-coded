---
paths:
  - ".github/**"
---

## CI/CD Conventions

- GitHub Actions job names must match kebab-case Nuke target names exactly (e.g., `lint-server-verify`)
- Path-based job gating with `dorny/paths-filter@v3`
- Externalize complex JS logic to `.github/scripts/` — no large inline scripts in YAML
