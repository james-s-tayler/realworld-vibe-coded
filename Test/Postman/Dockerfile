# Use the official .NET 10 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_HTTPS_PORT=

WORKDIR /app

COPY publish/ ./

ENTRYPOINT ["./Server.Web"]
