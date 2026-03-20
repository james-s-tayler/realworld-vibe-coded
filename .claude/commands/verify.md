Run full pre-commit verification. Execute each step sequentially and stop on first failure:

1. `./build.sh LintAllVerify` — If fails, run `./build.sh LintAllFix` then re-verify
2. `./build.sh BuildServer`
3. `./build.sh BuildClient`
4. `./build.sh TestServer`
5. `./build.sh TestClient`

Report any failures with details from `Reports/` and `Logs/` directories. Follow any instructions in error messages for accessing specific reports.
