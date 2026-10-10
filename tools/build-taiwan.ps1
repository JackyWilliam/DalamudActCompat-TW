param(
    [string] $ReferenceDirectory,
    [string] $OutputDirectory = (Join-Path $PSScriptRoot '../artifacts/taiwan'),
    [switch] $PrepareOnly,
    [switch] $SkipTests
)
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$pins = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'taiwan-dependencies.lock.json') -Raw | ConvertFrom-Json
if (-not $ReferenceDirectory) { $ReferenceDirectory = Join-Path $root '.deps/taiwan/dalamud' }
$refs = [IO.Path]::GetFullPath($ReferenceDirectory)
if (-not (Test-Path -LiteralPath (Join-Path $refs 'Dalamud.dll'))) {
    New-Item -ItemType Directory -Path $refs -Force | Out-Null
    $archive = Join-Path (Split-Path $refs) 'dalamud.7z'
    Invoke-WebRequest -Uri $pins.dalamudArchiveUrl -OutFile $archive
    if ((Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash -ne $pins.dalamudArchiveSha256) {
        throw 'Taiwan Dalamud archive checksum mismatch.'
    }
    & 7z x $archive "-o$refs" -y | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Taiwan Dalamud extraction failed; install 7-Zip.' }
}
# A local launcher folder must match the same pinned assembly set as CI. Merely
# checking API 13 would also accept international builds with different native layouts.
foreach ($property in $pins.referenceSha256.PSObject.Properties) {
    $file = Join-Path $refs $property.Name
    if (-not (Test-Path -LiteralPath $file) -or
        (Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash -ne $property.Value) {
        throw "Pinned Taiwan reference mismatch: $($property.Name)"
    }
}
$refs = $refs.TrimEnd('\', '/') + [IO.Path]::DirectorySeparatorChar
if ($env:GITHUB_ENV) { "DALAMUD_HOME=$refs" >> $env:GITHUB_ENV }
if ($PrepareOnly) { Write-Output $refs; return }

Push-Location $root
try {
    & dotnet build DalamudActCompat.slnx -c Release "-p:DalamudLibPath=$refs" -p:ShouldUnsetParentConfigurationAndPlatform=false
    if ($LASTEXITCODE -ne 0) { throw 'Taiwan solution build failed.' }
    $tfm = 'net9.0-windows10.0.17763.0'
    if (-not $SkipTests) {
        $checks = @(
            @('tests/DalamudActCompat.ScannerSmokeTests/bin/Release/net9.0-windows/DalamudActCompat.ScannerSmokeTests.exe'),
            @("tests/DalamudActCompat.PackageSmokeTests/bin/Release/$tfm/DalamudActCompat.PackageSmokeTests.exe"),
            @("tests/DalamudActCompat.HostSmokeTests/bin/Release/$tfm/DalamudActCompat.HostSmokeTests.exe",
              "tests/DalamudActCompat.HostSmokeTests/bin/Release/$tfm/DalamudActCompat.Host.exe", 'vendor/BundledActPlugins/triggernometry/Triggernometry.dll'),
            @('tests/DalamudActCompat.LegacyResourceSmokeTests/bin/Release/net9.0-windows/DalamudActCompat.LegacyResourceSmokeTests.exe',
              'vendor/BundledActPlugins/triggernometry/Triggernometry.dll', 'vendor/BundledActPlugins/postnamazu/PostNamazu.dll')
        )
        foreach ($check in $checks) {
            $arguments = @($check | Select-Object -Skip 1)
            & $check[0] @arguments
            if ($LASTEXITCODE -ne 0) { throw "Taiwan smoke failed: $($check[0])" }
        }
    }
    $out = [IO.Path]::GetFullPath($OutputDirectory)
    New-Item -ItemType Directory -Path $out -Force | Out-Null
    $package = Join-Path $out 'DalamudActCompat-TW-candidate.zip'
    Copy-Item -LiteralPath 'src/DalamudActCompat/bin/Release/DalamudActCompat/latest.zip' -Destination $package -Force
    $hash = (Get-FileHash -LiteralPath $package -Algorithm SHA256).Hash.ToLowerInvariant()
    "$hash  $([IO.Path]::GetFileName($package))" | Set-Content -LiteralPath "$package.sha256" -Encoding ascii
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'taiwan-dependencies.lock.json') -Destination $out -Force
    Write-Output "Taiwan candidate: $package ($hash)"
}
finally { Pop-Location }
