---
applyTo: "Makefile"
---

# Config and Helpers section contents
The Makefile should start with the following code:

    # ==================================================================================== #
    # CONFIG
    # ==================================================================================== #
    .DEFAULT_GOAL := help
    SERVER_SOLUTION ?= ./App/Server/Server.sln
    SERVER_PROJECT ?= ./App/Server/src/Server.Web/Server.Web.csproj
    CLIENT_SOLUTION ?= ./App/Client
    # the nuclear option - treat every target as .PHONY
    # this makes sense if you're not relying on make's incremental build features
    # and if you're just using make purely as a task runner
    .PHONY: %

    # ==================================================================================== #
    # HELPERS
    # ==================================================================================== #
    
    #HELP list all the available tasks
    help:
    # 1. (awk) If a line in the Makefile starts with #HELP, save the target line ($$0) into a variable (c) and skip to the next input line.
    # 2. (awk) If a line looks like a make target (e.g. it starts with [a-ZA-Z0-9] contains slashes etc and then has a colon) and we have a stored comment in c
    # 3. (awk) Then print the target line ($$0) and the stored comment (c) on one line, and clear c back to ""
    #    The above repeats for all targets in the file and produces output like the following which then gets cleaned up
    #    thing: dep1 dep2 #HELP this does a thing that depends on dep1 and dep2
    # 4. (sed) match everything between : and #HELP and replace it with just #HELP. This deletes any dependencies of the target from the output.
    # 5. (column) split the output into columns using tabs with #HELP as the separator. This removes #HELP from the output and formats into columns.
    @awk '/^#HELP/ {c=$$0; next} /^[a-zA-Z0-9_\/-]+:/ {if (c) {print $$0, c; c=""}}' $(MAKEFILE_LIST) | sed 's/:.*#HELP/\ #HELP/' | column -t -s '#HELP'
    
    
    #HELP confirm before running a destructive action
    confirm:
    @echo -n 'Are you sure? [y/N] ' && read ans && [ $${ans:-N} = y ]

## Sections
The Makefile should contain the following sections:

- CONFIG
- HELPERS
- LINTING
- BUILD
- TEST
- RUN-LOCAL

## HELP comments
Each target should have a comment starting with `#HELP` immediately above it describing what the target does

Example:

    #HELP dotnet build
    build/server:
    dotnet build "${SERVER_SOLUTION}"

## Target naming conventions
Targets should be named using the following conventions:

- targets should be lowercase
- words should be separated by forward slashes `/`
- targets should be grouped by section, e.g. `build/server`, `build/client`
- targets should be follow ${section}/${folder}` or `${section}/${folder}/${action}`