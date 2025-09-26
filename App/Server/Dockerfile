# Use the official .NET 9 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Update CA certificates and configure for NuGet SSL issues
RUN apt-get update && apt-get install -y ca-certificates && update-ca-certificates && apt-get clean && rm -rf /var/lib/apt/lists/*

# Copy all source files
COPY . ./

# Create a Docker-friendly nuget.config without LocalNuget source
RUN echo '<?xml version="1.0" encoding="utf-8"?><configuration><packageRestore><add key="enabled" value="True" /><add key="automatic" value="True" /></packageRestore><packageSources><clear /><add key="nuget.org" value="https://api.nuget.org/v3/index.json" /></packageSources><packageSourceMapping><packageSource key="nuget.org"><package pattern="*" /></packageSource></packageSourceMapping></configuration>' > nuget.config

# First, we need to remove the problematic SDK line temporarily
RUN sed -i 's/<Sdk Name="Microsoft.Build.CentralPackageVersions".*\/>//' src/Server.Web/Server.Web.csproj

# Set environment variables to help with SSL
ENV DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
ENV NUGET_RESTORE_MSBUILD_ARGS=/p:EnableMSBuildWarn=false

# Restore and publish the Web project
RUN dotnet restore src/Server.Web/Server.Web.csproj --configfile nuget.config
RUN dotnet publish src/Server.Web/Server.Web.csproj -c Release -o out

# Use the official .NET 9 runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy the published app
COPY --from=build /app/out .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:5000

# Expose port
EXPOSE 5000

# Start the application
ENTRYPOINT ["dotnet", "Server.Web.dll"]