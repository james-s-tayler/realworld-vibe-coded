# Use the official .NET 9 SDK image
FROM mcr.microsoft.com/dotnet/sdk:9.0
WORKDIR /app

# Copy everything
COPY . .

# Build the application
RUN dotnet build src/Server.Web/Server.Web.csproj -c Release

# Install openssl and curl
RUN apt-get update && apt-get install -y openssl curl && rm -rf /var/lib/apt/lists/*

# Generate self-signed certificate
RUN mkdir -p /https && \
    openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
      -keyout /https/aspnetapp.key \
      -out /https/aspnetapp.crt \
      -subj "/CN=localhost/O=Development/C=US" && \
    openssl pkcs12 -export -out /https/aspnetapp.pfx \
      -inkey /https/aspnetapp.key \
      -in /https/aspnetapp.crt \
      -passout pass:

# Set environment variables for HTTPS
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=https://+:5001
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=

# Expose HTTPS port
EXPOSE 5001

# Run the application with explicit URL override
ENTRYPOINT ["dotnet", "run", "--project", "src/Server.Web/Server.Web.csproj", "--configuration", "Release", "--no-build", "--urls", "https://0.0.0.0:5001"]