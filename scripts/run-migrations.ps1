param(
    [string]$StartupProject = "Company.Template.Api/Company.Template.Api.csproj",
    [string]$PersistenceProject = "Company.Template.Infrastructure.Persistence/Company.Template.Infrastructure.Persistence.csproj",
    [string]$Environment = "Development"
)

$ErrorActionPreference = 'Stop'

dotnet ef database update `
    --project $PersistenceProject `
    --startup-project $StartupProject `
    -- `
    --environment $Environment
