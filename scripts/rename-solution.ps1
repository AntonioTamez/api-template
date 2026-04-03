param(
    [Parameter(Mandatory = $true)]
    [string]$NewName
)

$ErrorActionPreference = 'Stop'

$oldName = 'Company.Template'

if ([string]::IsNullOrWhiteSpace($NewName)) {
    Write-Error 'New name cannot be empty.'
}

if ($NewName -eq $oldName) {
    Write-Host 'New name matches the current template name. Nothing to do.'
    exit 0
}

$excludedSegments = @('\.git\', '\.vs\', '\.idea\', '\.vscode\', '\bin\', '\obj\')
$textExtensions = @('.sln', '.csproj', '.props', '.targets', '.cs', '.json', '.md', '.yml', '.yaml', '.ps1', '.sh', '.dockerignore', '.gitignore', '.http')
$specialFileNames = @('Dockerfile', 'docker-compose.yml', '.env.example', 'PLAN.md')

function ShouldSkip([string]$path) {
    foreach ($segment in $excludedSegments) {
        if ($path -like "*$segment*") {
            return $true
        }
    }

    return $false
}

function ShouldProcessFile([System.IO.FileInfo]$file) {
    if (ShouldSkip($file.FullName)) {
        return $false
    }

    if ($specialFileNames -contains $file.Name) {
        return $true
    }

    return $textExtensions -contains $file.Extension
}

$files = Get-ChildItem -Recurse -File | Where-Object { ShouldProcessFile($_) }
$pattern = [regex]::Escape($oldName)

foreach ($file in $files) {
    $content = Get-Content -LiteralPath $file.FullName -Raw -ErrorAction Stop
    if ($content -notmatch $pattern) {
        continue
    }

    $updated = $content -replace $pattern, $NewName
    if ($updated -ne $content) {
        Set-Content -LiteralPath $file.FullName -Value $updated -Encoding UTF8
    }
}

$itemsToRename = Get-ChildItem -Recurse -File | Where-Object { $_.Name -like "*$oldName*" } |
    Sort-Object FullName -Descending

foreach ($item in $itemsToRename) {
    $newFileName = $item.Name.Replace($oldName, $NewName)
    Rename-Item -LiteralPath $item.FullName -NewName $newFileName
}

$directoriesToRename = Get-ChildItem -Recurse -Directory | Where-Object { $_.Name -like "*$oldName*" } |
    Sort-Object FullName -Descending

foreach ($dir in $directoriesToRename) {
    $newDirectoryName = $dir.Name.Replace($oldName, $NewName)
    Rename-Item -LiteralPath $dir.FullName -NewName $newDirectoryName
}

Write-Host "Renamed template artifacts from '$oldName' to '$NewName'."
