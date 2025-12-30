# Use the official .NET 10 SDK image for EF tools
FROM mcr.microsoft.com/dotnet/sdk:10.0

WORKDIR /app

# Install dotnet-ef tool first (version 10.* for .NET 10 compatibility)
# This layer is cached and only invalidates if the tool version changes
RUN dotnet tool install --global dotnet-ef --version 10.*

# Add dotnet tools to PATH
ENV PATH="${PATH}:/root/.dotnet/tools"

# Copy only the files needed for restore (props and csproj files for Server.Web and its dependencies)
# This layer is cached and only invalidates if project dependencies change
COPY App/Server/Directory.Build.props App/Server/
COPY App/Server/Directory.Packages.props App/Server/
COPY App/Server/src/Server.SharedKernel/Server.SharedKernel.csproj App/Server/src/Server.SharedKernel/
COPY App/Server/src/Server.Core/Server.Core.csproj App/Server/src/Server.Core/
COPY App/Server/src/Server.UseCases/Server.UseCases.csproj App/Server/src/Server.UseCases/
COPY App/Server/src/Server.Infrastructure/Server.Infrastructure.csproj App/Server/src/Server.Infrastructure/
COPY App/Server/src/Server.Web/Server.Web.csproj App/Server/src/Server.Web/
COPY App/Server/src/Server.Web.DevOnly/Server.Web.DevOnly.csproj App/Server/src/Server.Web.DevOnly/
COPY App/Server/analyzers/Server.Analyzers/Server.Analyzers.csproj App/Server/analyzers/Server.Analyzers/

# Restore packages for Server.Web (restores Server.Web and all its dependencies)
# This layer is cached unless project files or dependencies change
RUN dotnet restore App/Server/src/Server.Web/Server.Web.csproj

# Copy the entire application (source code for all projects)
# This layer invalidates whenever any source code changes
COPY . .

# Script to run migrations
COPY Test/Migrations/run-migrations.sh /app/run-migrations.sh
RUN chmod +x /app/run-migrations.sh

ENTRYPOINT ["/app/run-migrations.sh"]
