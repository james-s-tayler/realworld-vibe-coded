#!/usr/bin/env bash
set -e

dotnet run --project build/_build/_build.csproj -- "$@"