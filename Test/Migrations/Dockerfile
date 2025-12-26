# Use the official .NET 10 SDK image for EF tools
FROM mcr.microsoft.com/dotnet/sdk:10.0

WORKDIR /app

# Copy the entire application
COPY . .

# Restore packages to ensure all dependencies are available
RUN dotnet restore App/Server/Server.sln

# Install dotnet-ef tool (version 10.* for .NET 10 compatibility)
RUN dotnet tool install --global dotnet-ef --version 10.*

# Add dotnet tools to PATH
ENV PATH="${PATH}:/root/.dotnet/tools"

# Script to run migrations
COPY Test/Migrations/run-migrations.sh /app/run-migrations.sh
RUN chmod +x /app/run-migrations.sh

ENTRYPOINT ["/app/run-migrations.sh"]
