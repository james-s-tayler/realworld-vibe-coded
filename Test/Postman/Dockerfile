# Use the official .NET 9 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0

WORKDIR /app

# Copy the published application
COPY publish/ ./

# Install openssl and curl
RUN apt-get update && apt-get install -y openssl curl && rm -rf /var/lib/apt/lists/*

# Copy certificate generation script
COPY Test/Postman/generate-dev-cert.sh /app/generate-dev-cert.sh
RUN chmod +x /app/generate-dev-cert.sh

# Generate self-signed certificate
RUN /app/generate-dev-cert.sh

# Set environment variables for HTTPS
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=https://+:5001
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=

# Expose HTTPS port
EXPOSE 5001

# Run the published application
ENTRYPOINT ["./Server.Web"]
