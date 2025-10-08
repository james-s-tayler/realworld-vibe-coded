# Use the official .NET 9 SDK image for EF tools
FROM mcr.microsoft.com/dotnet/sdk:9.0

WORKDIR /app

# Copy the entire application
COPY . .

# Install dotnet-ef tool
RUN dotnet tool install --global dotnet-ef

# Add dotnet tools to PATH
ENV PATH="${PATH}:/root/.dotnet/tools"

# Script to run migrations
COPY Test/Migrations/run-migrations.sh /app/run-migrations.sh
RUN chmod +x /app/run-migrations.sh

ENTRYPOINT ["/app/run-migrations.sh"]
