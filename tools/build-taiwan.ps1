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
    # Release artifacts must not retain local source/PDB paths in nested vendor builds.
    & dotnet build DalamudActCompat.slnx -c Release "-p:DalamudLibPath=$refs" -p:ShouldUnsetParentConfigurationAndPlatform=false -p:DebugType=none -p:DebugSymbols=false
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
    # TW keeps its self-contained API 13 package. Audit the actual ZIP against this
    # build and its pinned resources so a fresh main DLL cannot hide a stale Host.
    $archive = [IO.Compression.ZipFile]::OpenRead($package)
    try {
        $groups = [ordered]@{}
        foreach ($group in @('host', 'BundledCactbot', 'BundledActPlugins')) {
            $files = @($archive.Entries | Where-Object { $_.FullName.Replace('\', '/').StartsWith("$group/") -and $_.Name } | Sort-Object FullName)
            if ($files.Count -eq 0) { throw "Taiwan package is missing resource group: $group" }
            $records = @()
            foreach ($entry in $files) {
                $relative = $entry.FullName.Replace('\', '/').Substring($group.Length + 1)
                if ($group -eq 'host') {
                    $source = "src/DalamudActCompat.Host/bin/Release/$tfm/$relative"
                    if ($relative -eq 'System.Speech.dll') {
                        $source = "src/DalamudActCompat.Host/bin/Release/$tfm/runtimes/win/lib/net9.0/System.Speech.dll"
                    }
                } else { $source = "vendor/$group/$relative" }
                $stream = $entry.Open()
                try { $entryHash = (Get-FileHash -InputStream $stream -Algorithm SHA256).Hash.ToLowerInvariant() }
                finally { $stream.Dispose() }
                if (-not (Test-Path -LiteralPath $source) -or (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash -ne $entryHash) {
                    throw "Taiwan packaged resource differs from current build/source: $($entry.FullName)"
                }
                $records += [ordered]@{ path = $relative; size = $entry.Length; sha256 = $entryHash }
            }
            $content = ($records | ForEach-Object { "$($_.path)`t$($_.sha256)`n" }) -join ''
            $algorithm = [Security.Cryptography.SHA256]::Create()
            try { $contentHash = [Convert]::ToHexString($algorithm.ComputeHash([Text.Encoding]::UTF8.GetBytes($content))).ToLowerInvariant() }
            finally { $algorithm.Dispose() }
            $groups[$group] = [ordered]@{ contentSha256 = $contentHash; files = $records }
        }
        $names = @($archive.Entries | ForEach-Object { $_.FullName.Replace('\', '/') })
        if ($names -notcontains 'host/DalamudActCompat.Host.dll' -or
            $names -notcontains 'host/DalamudActCompat.Host.exe') { throw 'Taiwan Host assembly is missing.' }
        if ($names.Where({ $_ -match '(?i)(^|/)matcha/' }).Count) { throw 'Taiwan package includes Matcha.' }
        $reader = [IO.StreamReader]::new($archive.GetEntry('DalamudActCompat.json').Open())
        try { $manifest = $reader.ReadToEnd() | ConvertFrom-Json } finally { $reader.Dispose() }
        if ($manifest.DalamudApiLevel -ne 13 -or $manifest.RepoUrl -ne 'https://github.com/JackyWilliam/DalamudActCompat-TW') {
            throw 'Taiwan package identity mismatch.'
        }
        $audit = [ordered]@{ schemaVersion = 1; version = $manifest.AssemblyVersion; dalamudApiLevel = 13; resources = $groups }
        $audit | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $out 'resource-content-hashes.json') -Encoding utf8
    } finally { $archive.Dispose() }
    $hash = (Get-FileHash -LiteralPath $package -Algorithm SHA256).Hash.ToLowerInvariant()
    "$hash  $([IO.Path]::GetFileName($package))" | Set-Content -LiteralPath "$package.sha256" -Encoding ascii
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'taiwan-dependencies.lock.json') -Destination $out -Force
    Write-Output "Taiwan candidate: $package ($hash)"
}
finally { Pop-Location }
