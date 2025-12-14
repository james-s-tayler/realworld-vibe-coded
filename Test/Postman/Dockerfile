# Stage 1: Generate HTTPS certificate
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS cert-generator
ARG CERT_PASSWORD=DevPassword123!
RUN mkdir -p /https && dotnet dev-certs https -ep /https/aspnetapp.pfx -p "$CERT_PASSWORD"

# Stage 2: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0

WORKDIR /app

# Copy the published application
COPY publish/ ./

# Copy the HTTPS certificate from the cert-generator stage
COPY --from=cert-generator /https/aspnetapp.pfx /https/aspnetapp.pfx

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Run the published application
ENTRYPOINT ["./Server.Web"]
