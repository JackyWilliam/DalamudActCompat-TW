using System.Text.RegularExpressions;

namespace DalamudActCompat.Infrastructure.Cloud;

internal static class CloudClientIdentity
{
    internal static string? NormalizeGameVersion(string? value)
    {
        var version = value?.Trim();
        return version is not null && Regex.IsMatch(version, @"\A[0-9]{4}\.[0-9]{2}\.[0-9]{2}\.[0-9]{4}\.[0-9]{4}\z")
            ? version : null;
    }

    internal static string? ReadRunningGameVersion(string? executablePath)
    {
        // Match the parser's version source beside the running game executable.
        // The build's supported version is not evidence of the installed client.
        if (string.IsNullOrWhiteSpace(executablePath) ||
            Path.GetFileName(executablePath) is not { } filename ||
            !(filename.Equals("ffxiv_dx11.exe", StringComparison.OrdinalIgnoreCase) ||
              filename.Equals("ffxiv.exe", StringComparison.OrdinalIgnoreCase)))
            return null;
        try
        {
            var path = Path.Combine(Path.GetDirectoryName(executablePath)!, "ffxivgame.ver");
            return File.Exists(path) ? NormalizeGameVersion(File.ReadAllText(path)) : null;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            // Support metadata must never prevent login when the version file is unavailable.
            return null;
        }
    }
}
