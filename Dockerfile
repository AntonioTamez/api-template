# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["global.json", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Build.targets", "./"]
COPY ["Company.Template.sln", "./"]
COPY src/ ./src/
COPY tests/ ./tests/

RUN dotnet restore src/Domain/Company.Template.Domain.csproj && \
    dotnet build src/Domain/Company.Template.Domain.csproj -c Release --no-restore && \
    dotnet restore src/Application/Company.Template.Application.csproj && \
    dotnet build src/Application/Company.Template.Application.csproj -c Release --no-restore && \
    dotnet restore src/Infrastructure.Persistence/Company.Template.Infrastructure.Persistence.csproj && \
    dotnet build src/Infrastructure.Persistence/Company.Template.Infrastructure.Persistence.csproj -c Release --no-restore && \
    dotnet restore src/Infrastructure/Company.Template.Infrastructure.csproj && \
    dotnet build src/Infrastructure/Company.Template.Infrastructure.csproj -c Release --no-restore && \
    dotnet restore src/Api/Company.Template.Api.csproj && \
    dotnet build src/Api/Company.Template.Api.csproj -c Release --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /src/src/Api/bin/Release/net8.0/ .
ENTRYPOINT ["dotnet", "Company.Template.Api.dll"]
