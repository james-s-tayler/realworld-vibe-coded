---
mode: 'agent'
model: GPT-4.1
description: 'Create a new server project following the established conventions and structure.'
---

1. run `dotnet new install Ardalis.CleanArchitecture.Template`
2. cd to the `App` directory
3. run `dotnet new clean-arch -o Server --aspire false`
4. cd back to the root directory
5. check the backend project builds and tests pass with `dotnet build`
6. check the backend tests pass with `dotnet test`
7. Add targets to the Nuke build system to build and test the new project