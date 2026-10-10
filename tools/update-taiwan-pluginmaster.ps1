param(
    [Parameter(Mandatory)][string] $Package,
    [Parameter(Mandatory)][ValidatePattern('^v\d+\.\d+\.\d+\.\d+$')][string] $ReleaseTag,
    [string] $OutputPath = (Join-Path $PSScriptRoot '../repo/pluginmaster.json')
)
$ErrorActionPreference = 'Stop'
$repository = 'https://github.com/JackyWilliam/DalamudActCompat-TW'
$archive = [IO.Compression.ZipFile]::OpenRead([IO.Path]::GetFullPath($Package))
try {
    $entry = $archive.GetEntry('DalamudActCompat.json')
    if (-not $entry) { throw 'Taiwan package manifest is missing.' }
    $reader = [IO.StreamReader]::new($entry.Open())
    try { $manifest = $reader.ReadToEnd() | ConvertFrom-Json } finally { $reader.Dispose() }
    if ($manifest.InternalName -ne 'DalamudActCompat' -or
        $manifest.RepoUrl -ne $repository -or
        $manifest.DalamudApiLevel -ne 13 -or
        $manifest.ApplicableVersion -ne '2026.09.14.0000.0000' -or
        $manifest.MinimumDalamudVersion -ne '13.0.0.16') {
        throw 'Only the pinned Taiwan API 13 package can enter this repository.'
    }
    if ($archive.Entries.Where({ $_.FullName -match '(?i)(^|/)matcha/' }).Count) {
        throw 'Matcha is excluded from the Taiwan distribution.'
    }
    $version = [version]$manifest.AssemblyVersion
    if ($ReleaseTag -ne "v$version") {
        throw 'Release tag and package assembly version disagree.'
    }
} finally { $archive.Dispose() }

# Use the real assembly version so installed candidates compare correctly on
# update. Candidate status remains explicit in the description and GitHub release.
$download = "$repository/releases/download/$ReleaseTag/DalamudActCompat-TW-candidate.zip"
$feed = [ordered]@{
    Author = $manifest.Author
    Name = 'Dalamud ACT Compat (TW)'
    InternalName = $manifest.InternalName
    AssemblyVersion = $version.ToString()
    TestingAssemblyVersion = $null
    Description = '台服 API 13 專用測試候選版，預設繁體中文。整合戰鬥統計、Cactbot、HTML 懸浮窗、快捷鍵與 ACT 擴充套件；不包含 Matcha。遊戲內相容性仍待驗收。'
    Punchline = '台服專用 ACT 相容平台，預設繁體中文。'
    RepoUrl = $repository
    ApplicableVersion = $manifest.ApplicableVersion
    MinimumDalamudVersion = $manifest.MinimumDalamudVersion
    DalamudApiLevel = 13
    IsHide = $false
    IsTestingExclusive = $false
    DownloadLinkInstall = $download
    DownloadLinkUpdate = $download
    DownloadLinkTesting = $download
    LastUpdate = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
    Changelog = '首次台服獨立測試訂閱。預設繁體中文，保留既有語言選擇；提供完整台服候選功能，不包含 Matcha。'
    CanUnloadAsync = $manifest.CanUnloadAsync
    IconUrl = 'https://raw.githubusercontent.com/JackyWilliam/DalamudActCompat-TW/main/src/DalamudActCompat/Assets/act-logo.jpg'
}
# Emit exactly one Taiwan entry; historical CN metadata and its repair utility
# remain reference-only and must never become install/update targets here.
$json = ConvertTo-Json -InputObject @($feed) -Depth 10
[IO.File]::WriteAllText([IO.Path]::GetFullPath($OutputPath), $json + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))
Write-Output "Updated Taiwan testing repository: $OutputPath"
