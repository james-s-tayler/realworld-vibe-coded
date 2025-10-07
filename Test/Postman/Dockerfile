# Use the official .NET 9 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0

WORKDIR /app

# Copy the published application
COPY publish/ ./

# Set environment variables to force HTTP only
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_HTTPS_PORT=

# Install curl for health checks and dotnet-coverage tool
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/* && \
    dotnet tool install --global dotnet-coverage

# Add dotnet tools to PATH
ENV PATH="${PATH}:/root/.dotnet/tools"

# Expose port
EXPOSE 5000

# Run the published application with code coverage collection
ENTRYPOINT ["dotnet-coverage", "collect", "--output", "/coverage/coverage.cobertura.xml", "--output-format", "cobertura", "--session-id", "postman-coverage", "./Server.Web"]
