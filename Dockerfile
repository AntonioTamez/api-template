# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["global.json", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Build.targets", "./"]
COPY ["Company.Template.sln", "./"]
COPY Company.Template.Api/Company.Template.Api.csproj Company.Template.Api/
COPY Company.Template.Application/Company.Template.Application.csproj Company.Template.Application/
COPY Company.Template.Domain/Company.Template.Domain.csproj Company.Template.Domain/
COPY Company.Template.Infrastructure/Company.Template.Infrastructure.csproj Company.Template.Infrastructure/
COPY Company.Template.Infrastructure.Persistence/Company.Template.Infrastructure.Persistence.csproj Company.Template.Infrastructure.Persistence/
COPY Company.Template.Domain.UnitTests/Company.Template.Domain.UnitTests.csproj Company.Template.Domain.UnitTests/
COPY Company.Template.Application.UnitTests/Company.Template.Application.UnitTests.csproj Company.Template.Application.UnitTests/
COPY Company.Template.Infrastructure.IntegrationTests/Company.Template.Infrastructure.IntegrationTests.csproj Company.Template.Infrastructure.IntegrationTests/

RUN dotnet restore Company.Template.sln

COPY . .
WORKDIR /src/Company.Template.Api
RUN dotnet publish Company.Template.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Company.Template.Api.dll"]
