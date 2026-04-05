# syntax=docker/dockerfile:1.7
# SDK version must match global.json. Runtime version = SDK patch (8.0.4XX → 8.0.XX).
# Update both together when bumping global.json.

FROM mcr.microsoft.com/dotnet/sdk:8.0.419 AS build
WORKDIR /src

COPY global.json Directory.Build.props Directory.Build.targets Company.Template.sln ./
COPY src/ ./src/
COPY tests/ ./tests/

RUN dotnet restore
RUN dotnet publish src/Api/Company.Template.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0.19 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Company.Template.Api.dll"]
