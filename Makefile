# ==================================================================================== #
# CONFIG
# ==================================================================================== #
.DEFAULT_GOAL := help
SERVER_SOLUTION ?= ./App/Server/Server.sln
SERVER_PROJECT ?= ./App/Server/src/Server.Web/Server.Web.csproj
CLIENT_SOLUTION ?= ./App/Client
.PHONY: %

# ==================================================================================== #
# HELPERS
# ==================================================================================== #
#HELP list all the available tasks
help:
	@awk '/^#HELP/ {c=$$0; next} /^[a-zA-Z0-9_\/-]+:/ {if (c) {print $$0, c; c=""}}' $(MAKEFILE_LIST) | sed 's/:.*#HELP/ #HELP/' | column -t -s '#HELP'

#HELP confirm before running a destructive action
confirm:
	@echo -n 'Are you sure? [y/N] ' && read ans && [ $${ans:-N} = y ]

# ==================================================================================== #
# LINTING
# ==================================================================================== #
#HELP verify backend formatting & analyzers (no changes). Fails if issues found
lint/server:
	@echo "Running dotnet format (verify only) on $(SERVER_SOLUTION) ..."
	dotnet format --verify-no-changes "${SERVER_SOLUTION}"

#HELP lint client code
lint/client:
	@echo "No client linting configured yet."

# ==================================================================================== #
# BUILD
# ==================================================================================== #
#HELP dotnet build (backend)
build/server:
	dotnet build "${SERVER_SOLUTION}"

#HELP build client (frontend)
build/client:
	@echo "No client build configured yet."

# ==================================================================================== #
# TEST
# ==================================================================================== #
#HELP run backend tests
test/server:
	dotnet test "${SERVER_SOLUTION}"

#HELP run client tests
test/client:
	@echo "No client tests configured yet."

# ==================================================================================== #
# RUN-LOCAL
# ==================================================================================== #
#HELP run backend locally
run-local/server:
	dotnet run --project "${SERVER_PROJECT}"

#HELP run client locally
run-local/client:
	@echo "No client run-local configured yet."

# ==================================================================================== #
# DB
# ==================================================================================== #
#HELP delete local sqlite database (idempotent)
db/reset:
	@[ "$(FORCE)" = "1" ] || $(MAKE) confirm
	@echo "Deleting App/Server/src/Server.Web/database.sqlite ..."
	rm -f -- App/Server/src/Server.Web/database.sqlite
	@echo "Done."
