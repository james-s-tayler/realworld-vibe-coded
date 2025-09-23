# ==================================================================================== #
# CONFIG
# ==================================================================================== #
.DEFAULT_GOAL := help
SERVER_SOLUTION ?= ./App/Server/Server.sln
SERVER_PROJECT ?= ./App/Server/src/Server.Web/Server.Web.csproj
CLIENT_SOLUTION ?= ./App/Client
POSTMAN_COMPOSE_FILE ?= ./Infra/Postman/docker-compose.yml
export FOLDER
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
    # 4. (sed) match everything between : and #HELP and replace it with just #. This deletes any dependencies of the target from the output.
    # 5. (column) split the output into columns using tabs with # as the separator. This removes # from the output and formats into columns.
	@awk '/^#HELP/ {c=$$0; next} /^[a-zA-Z0-9_\/-]+:/ {if (c) {print $$0, c; c=""}}' $(MAKEFILE_LIST) | sed 's/:.*#HELP/\ #/' | column -t -s '#'

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

#HELP lint makefile
lint/make: lint/make/help/hash lint/make/help/count
	@echo "Makefile linting passed."

#HELP verify that help comment itself does not contain hash character
lint/make/help/hash:
	@# 1. Get all the lines that start with #HELP
	@# 2. Remove the #HELP part to leave just the comment
	@# 3. Grep for any lines that contain a # character
	@# 4. If any lines are found, print an error and exit with a failure code
	@h=$$(grep '^#HELP' $(MAKEFILE_LIST) | sed 's/^#HELP//' | grep "#" | wc -l); \
    [ $$h -eq 0 ] || { echo "Error ❌❌❌ $$h #HELP comment(s) contains a # character. This is not allowed as it breaks the help target."; exit 1; }

#HELP check that each target in the make file has help docs
lint/make/help/count:
	@# 1. Count the number of lines that start with #HELP
	@# 2. Gather all the lines that look like make targets (e.g. start with [a-ZA-Z0-9] and contain slashes etc and then have a colon)
	@# 3. Exclude any lines that start with . or % (these are special targets in make e.g. .PHONY)
	@# 4. Remove everything after the colon (the dependencies) to leave just the target name
	@# 5. Sort the target names and remove duplicates (this is required to avoid double counting targets E.g postman targets)
	@# 6. Count the number of unique targets
	@# 7. Compare the two counts and if they don't match, print an error
	@h=$$(grep -c '^#HELP' $(MAKEFILE_LIST)); \
	t=$$(grep -E '^[A-Za-z0-9_/.-]+:' $(MAKEFILE_LIST) | grep -vE '(^\.|%)' | sed 's/:.*//' | sort | uniq | wc -l); \
	[ $$h -eq $$t ] || { echo "Error ❌❌❌ targets=$$t but #HELP=$$h. Every target needs a #HELP comment directly above it."; exit 1; }

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

#HELP helper utility to prep for postman tests (reset db, stop any background backend, start backend, wait for it to be up)
test/server/postman/prep: db/reset/force run-local/server/background/stop run-local/server/background test/server/ping 

#HELP run postman tests
test/server/postman: test/server/postman/prep
	@docker_exit_code=0; \
	FOLDER=$(FOLDER) docker compose -f $(POSTMAN_COMPOSE_FILE) up --abort-on-container-exit || docker_exit_code=$$?; \
	$(MAKE) run-local/server/background/stop; \
	exit $$docker_exit_code
	
#HELP run postman tests in the Auth folder
test/server/postman/auth: FOLDER=Auth
test/server/postman/auth: test/server/postman

#HELP run postman tests in the ArticlesEmpty folder
test/server/postman/articles-empty: FOLDER=ArticlesEmpty
test/server/postman/articles-empty: test/server/postman

#HELP run postman tests in the Article folder
test/server/postman/article: FOLDER=Article
test/server/postman/article: test/server/postman

#HELP run postman tests in the FeedAndArticles folder
test/server/postman/feed: FOLDER=FeedAndArticles
test/server/postman/feed: test/server/postman

#HELP ping backend to see if it's up (requires backend running in background)
test/server/ping:
	@T=60; URL=https://localhost:57679/swagger/index.html; \
	for i in $$(seq 1 $$T); do \
	    echo "Pinging $$URL (attempt $$i of $$T) ..."; \
		status=$$(curl -k -s -o /dev/null -w "%{http_code}" $$URL); \
		if [ "$$status" -eq 200 ]; then \
			exit 0; \
		fi; \
		sleep 1; \
	done; \
	exit 1

#HELP run client tests
test/client:
	@echo "No client tests configured yet."

# ==================================================================================== #
# RUN-LOCAL
# ==================================================================================== #
#HELP run backend locally
run-local/server:
	dotnet run --project "${SERVER_PROJECT}"

#HELP run backend in the background (for local development)
run-local/server/background:
	dotnet run --project "${SERVER_PROJECT}" &

#HELP stop background backend (for local development)
run-local/server/background/stop:
	@echo "Killing Server.Web ..."
	-pkill dotnet || true
	@echo "Done."

#HELP run client locally
run-local/client:
	@echo "No client run-local configured yet."

# ==================================================================================== #
# DB
# ==================================================================================== #
#HELP delete local sqlite database (confirm or FORCE=1 to skip)
db/reset:
	@[ "$(FORCE)" = "1" ] || $(MAKE) confirm
	@echo "Deleting App/Server/src/Server.Web/database.sqlite ..."
	rm -f -- App/Server/src/Server.Web/database.sqlite
	@echo "Done."

#HELP delete local sqlite database (no confirmation)
db/reset/force:
	@echo "Deleting App/Server/src/Server.Web/database.sqlite ..."
	rm -f -- App/Server/src/Server.Web/database.sqlite
	@echo "Done."