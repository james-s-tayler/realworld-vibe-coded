---
description: Run full pre-commit verification. Use before committing to ensure all linting, builds, and tests pass. Execute each step sequentially and stop on first failure.
---

Run full pre-commit verification. Execute each step sequentially and stop on first failure:

1. `./build.sh LintAllVerify --agent` — If fails, run `./build.sh LintAllFix --agent` then re-verify
2. `./build.sh BuildServer --agent`
3. `./build.sh BuildClient --agent`
4. `./build.sh TestServer --agent`
5. `./build.sh TestClient --agent`

Report any failures with details from `Reports/` and `Logs/` directories. Follow any instructions in error messages for accessing specific reports.
