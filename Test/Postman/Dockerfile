# Use the official .NET 9 SDK image (needed for dotnet-coverage tool)
FROM mcr.microsoft.com/dotnet/sdk:9.0

WORKDIR /app

# Copy the published application
COPY publish/ ./

# Set environment variables to force HTTP only
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_HTTPS_PORT=

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Install dotnet-coverage tool
RUN dotnet tool install --global dotnet-coverage

# Add dotnet tools to PATH
ENV PATH="${PATH}:/root/.dotnet/tools"

# Expose port
EXPOSE 5000

# Run with dotnet-coverage directly
# The container will run as root, which has permissions to write to /coverage
ENTRYPOINT ["dotnet-coverage", "collect", \
    "--output", "/coverage/coverage.cobertura.xml", \
    "--output-format", "cobertura", \
    "dotnet", "Server.Web.dll"]
