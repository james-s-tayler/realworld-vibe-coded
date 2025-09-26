# Use the official .NET 9 SDK image that includes both runtime and SDK
FROM mcr.microsoft.com/dotnet/sdk:9.0
WORKDIR /app

# Copy everything
COPY . .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_HTTPS_PORT=""

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Expose port
EXPOSE 5000

# Use dotnet run directly to avoid build issues
CMD ["dotnet", "run", "--project", "src/Server.Web/Server.Web.csproj", "--urls", "http://+:5000"]