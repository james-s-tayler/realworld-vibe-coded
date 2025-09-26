#!/bin/bash

# Postman test script that mimics the Makefile behavior exactly
# This ensures reliability for both local development and CI

set -e  # Exit on error

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
cd "$ROOT_DIR"

echo "🚀 Starting Postman API tests..."

# Step 1: Setup reports directory
echo "📁 Setting up reports directory..."
mkdir -p reports
rm -f reports/newman-report.json

# Step 2: Reset database (like Makefile: db/reset/force)
echo "🗄️ Resetting database..."
rm -f App/Server/src/Server.Web/database.sqlite

# Step 3: Stop any existing servers (like Makefile: run-local/server/background/stop) 
echo "🛑 Stopping any existing servers..."
pkill dotnet || true

# Step 4: Start server in background (like Makefile: run-local/server/background)
echo "🏃 Starting server in background..."
dotnet run --project "./App/Server/src/Server.Web/Server.Web.csproj" &
SERVER_PID=$!

# Function to cleanup server on exit
cleanup() {
    echo "🧹 Cleaning up server..."
    pkill dotnet || true
}
trap cleanup EXIT

# Step 5: Wait for server to be ready (like Makefile: test/server/ping)
echo "🏓 Waiting for server to be ready..."
T=60
URL="https://localhost:57679/swagger/index.html"

for i in $(seq 1 $T); do
    echo "Pinging $URL (attempt $i of $T) ..."
    status=$(curl -k -s -o /dev/null -w "%{http_code}" $URL || echo "000")
    if [ "$status" -eq 200 ]; then
        echo "✅ Server is ready!"
        break
    fi
    sleep 1
    if [ $i -eq $T ]; then
        echo "❌ Server failed to start within timeout"
        exit 1
    fi
done

# Step 6: Run Newman tests via Docker Compose (like Makefile: test/server/postman)
echo "🐳 Running Postman tests via Docker Compose..."
docker_exit_code=0
FOLDER="${FOLDER}" docker compose -f ./Infra/Postman/docker-compose.yml up --abort-on-container-exit || docker_exit_code=$?

# Report results
if [ $docker_exit_code -eq 0 ]; then
    echo "✅ Postman tests passed!"
else
    echo "❌ Postman tests failed (exit code: $docker_exit_code)"
fi

# Exit with same code as docker (matching Makefile behavior)
exit $docker_exit_code