# Use the official .NET 9 SDK image (needed for dotnet-coverage tool)
FROM mcr.microsoft.com/dotnet/sdk:9.0

WORKDIR /app

# Copy the published application
COPY publish/ ./

# Copy coverage settings
COPY Test/Postman/coverage.runsettings ./

# Set environment variables to force HTTP only
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_HTTPS_PORT=

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Install dotnet-coverage tool for server mode
RUN dotnet tool install --global dotnet-coverage

# Add dotnet tools to PATH
ENV PATH="${PATH}:/root/.dotnet/tools"

# Create a script to start coverage collection server in background, then connect the app
RUN echo '#!/bin/bash\n\
set -e\n\
# Start dotnet-coverage server in background with settings file\n\
dotnet-coverage collect --server-mode --session-id postman-api-coverage --background --settings /app/coverage.runsettings --output /tmp/coverage.coverage &\n\
echo "Waiting for coverage server to start..."\n\
sleep 3\n\
# Connect the app to the coverage session (settings are already configured in server)\n\
echo "Starting app connected to coverage session..."\n\
exec dotnet-coverage connect postman-api-coverage dotnet Server.Web.dll\n\
' > /app/run-with-coverage.sh && chmod +x /app/run-with-coverage.sh

# Expose port
EXPOSE 5000

# Run the script
ENTRYPOINT ["/app/run-with-coverage.sh"]
