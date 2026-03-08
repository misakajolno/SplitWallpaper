$ErrorActionPreference = 'Stop'

$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptDirectory
$localDotnetPath = Join-Path $projectRoot '.dotnet/dotnet.exe'
$appProject = Join-Path $projectRoot 'src/SplitWallpaper.App/SplitWallpaper.App.csproj'
$outputDirectory = Join-Path $projectRoot 'release/framework-dependent'

if (Test-Path $localDotnetPath) {
    $dotnetCommand = $localDotnetPath
}
else {
    $dotnetOnPath = Get-Command 'dotnet' -ErrorAction SilentlyContinue
    if ($null -eq $dotnetOnPath) {
        throw "dotnet CLI not found. Install .NET 8 SDK first: https://dotnet.microsoft.com/download/dotnet/8.0"
    }

    $dotnetCommand = $dotnetOnPath.Source
}

if (-not (Test-Path $appProject)) {
    throw "App project not found at: $appProject"
}

if (Test-Path $outputDirectory) {
    Get-ChildItem -Path $outputDirectory -Force | Remove-Item -Recurse -Force
}
else {
    New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null
}

Write-Host "Using dotnet from: $dotnetCommand"

& $dotnetCommand publish $appProject `
    -c Release `
    -r win-x64 `
    --self-contained false `
    -o $outputDirectory

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE"
}

Write-Host "Published framework-dependent package to: $outputDirectory"
