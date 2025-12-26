# Use the official .NET 10 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

# Copy the published application
COPY publish/ ./

# Set environment variables to force HTTP only
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_HTTPS_PORT=

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Expose port
EXPOSE 5000

# Run the published application
ENTRYPOINT ["./Server.Web"]
