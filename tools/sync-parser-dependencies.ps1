param(
    [Parameter(Mandatory = $false)]
    [switch] $Check
)

$ErrorActionPreference = "Stop"

$projectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$iinactProjectPath = Join-Path $projectRoot "vendor/IINACT/IINACT/IINACT.csproj"
$dependencyDirectory = Join-Path $projectRoot "vendor/IINACT/external_dependencies"
# This standalone checkout must never pick the CN launcher reference folder.
$dalamudDirectory = if ($env:DALAMUD_HOME) { $env:DALAMUD_HOME } else { Join-Path $projectRoot '.deps/taiwan/dalamud' }
& (Join-Path $PSScriptRoot 'build-taiwan.ps1') -ReferenceDirectory $dalamudDirectory -PrepareOnly | Out-Null
$dalamudDirectory = [IO.Path]::GetFullPath($dalamudDirectory).TrimEnd("\", "/") + [IO.Path]::DirectorySeparatorChar
[xml] $iinactProject = Get-Content -LiteralPath $iinactProjectPath -Raw
$iinactVersion = [string] $iinactProject.Project.PropertyGroup.Version |
    Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
    Select-Object -First 1
if ([string]::IsNullOrWhiteSpace($iinactVersion)) {
    throw "Could not read the IINACT version from $iinactProjectPath."
}

$arguments = @(
    "run",
    "--project",
    (Join-Path $projectRoot "tools/ParserDependencySync/ParserDependencySync.csproj"),
    "-p:DalamudLibPath=$dalamudDirectory",
    "--",
    $dependencyDirectory,
    $iinactVersion
)
if ($Check) {
    $arguments += "--check"
}

& dotnet $arguments
if ($LASTEXITCODE -ne 0) {
    throw "Parser dependency synchronization failed with exit code $LASTEXITCODE."
}
