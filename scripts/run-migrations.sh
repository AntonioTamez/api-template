#!/usr/bin/env bash
set -euo pipefail

STARTUP_PROJECT=${1:-Company.Template.Api/Company.Template.Api.csproj}
PERSISTENCE_PROJECT=${2:-Company.Template.Infrastructure.Persistence/Company.Template.Infrastructure.Persistence.csproj}
ENVIRONMENT=${3:-Development}

dotnet ef database update \
  --project "$PERSISTENCE_PROJECT" \
  --startup-project "$STARTUP_PROJECT" \
  -- \
  --environment "$ENVIRONMENT"
