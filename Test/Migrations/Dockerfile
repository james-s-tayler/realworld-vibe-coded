FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ENV PATH="${PATH}:/root/.dotnet/tools"

RUN dotnet tool install --global dotnet-ef --version 10.*

WORKDIR /app

COPY App/Server/Directory.Build.props App/Server/
COPY App/Server/Directory.Packages.props App/Server/
COPY App/Server/analyzers/Server.Analyzers/Server.Analyzers.csproj App/Server/analyzers/Server.Analyzers/
COPY App/Server/src/Server.Web.DevOnly/Server.Web.DevOnly.csproj App/Server/src/Server.Web.DevOnly/
COPY App/Server/src/Server.SharedKernel/Server.SharedKernel.csproj App/Server/src/Server.SharedKernel/
COPY App/Server/src/Server.Core/Server.Core.csproj App/Server/src/Server.Core/
COPY App/Server/src/Server.Infrastructure/Server.Infrastructure.csproj App/Server/src/Server.Infrastructure/
COPY App/Server/src/Server.Web/Server.Web.csproj App/Server/src/Server.Web/
COPY App/Server/src/Server.UseCases/Server.UseCases.csproj App/Server/src/Server.UseCases/

RUN --mount=type=cache,id=nuget-http,target=/root/.local/share/NuGet/v3-cache \
    dotnet restore App/Server/src/Server.Web/Server.Web.csproj

COPY . .

RUN --mount=type=cache,id=nuget-http,target=/root/.local/share/NuGet/v3-cache \
    dotnet build App/Server/src/Server.Web/Server.Web.csproj --no-restore

FROM build AS apply-migrations
COPY Test/Migrations/run-apply-migrations.sh /app/run-script.sh
RUN chmod +x /app/run-script.sh
ENTRYPOINT ["/app/run-script.sh"]

FROM build AS generate-idempotent
COPY Test/Migrations/run-generate-idempotent.sh /app/run-script.sh
RUN chmod +x /app/run-script.sh
ENTRYPOINT ["/app/run-script.sh"]

FROM build AS verify-idempotent
COPY Test/Migrations/run-verify-idempotent.sh /app/run-script.sh
RUN chmod +x /app/run-script.sh
ENTRYPOINT ["/app/run-script.sh"]
