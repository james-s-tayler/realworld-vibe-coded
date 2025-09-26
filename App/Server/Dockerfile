# Use the official .NET 9 SDK image
FROM mcr.microsoft.com/dotnet/sdk:9.0
WORKDIR /app

# Copy everything
COPY . .

# Build the application
RUN dotnet build src/Server.Web/Server.Web.csproj -c Release

# Set environment variables to force HTTP only
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_HTTPS_PORT=

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Expose port
EXPOSE 5000

# Run the application with explicit URL override
ENTRYPOINT ["dotnet", "run", "--project", "src/Server.Web/Server.Web.csproj", "--configuration", "Release", "--no-build", "--urls", "http://0.0.0.0:5000"]