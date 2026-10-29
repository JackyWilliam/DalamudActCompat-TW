using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DalamudActCompat.Infrastructure.Cloud;

internal static class CloudClientIdentitySmokeTests
{
    internal static async Task RunAsync(string root)
    {
        const string build = "2026.09.14.0000.0000";
        var directory = Path.Combine(root, "client-identity");
        Directory.CreateDirectory(directory);
        var executable = Path.Combine(directory, "ffxiv_dx11.exe");
        var versionFile = Path.Combine(directory, "ffxivgame.ver");
        Check(CloudClientIdentity.ReadRunningGameVersion(executable) is null, "Missing game version was fabricated.");
        File.WriteAllText(versionFile, build + "\n");
        Check(CloudClientIdentity.ReadRunningGameVersion(executable) == build, "Installed game build was not read.");
        Check(CloudClientIdentity.ReadRunningGameVersion(Path.Combine(directory, "test.exe")) is null, "A test host was mistaken for FFXIV.");
        File.WriteAllText(versionFile, "2026.09.14.0000.0000\r\nX-Other: injected");
        Check(CloudClientIdentity.ReadRunningGameVersion(executable) is null, "Malformed version could become an HTTP header.");
        using var handler = new CaptureHandler();
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://isolated.test/") };
        using var api = new CloudApiClient(http, gameVersion: build);
        var login = await api.LoginAsync("fixture", "isolated-password", "fixture-device", default);
        Check(login.User.Id == "fixture", "Login response changed.");
        await api.ValidateSessionAsync("fixture-token", default);
        // The header reports the loaded plugin assembly, so the expectation follows each release bump.
        var pluginVersion = typeof(CloudApiClient).Assembly.GetName().Version!.ToString(4);
        Check(handler.Reports.Count == 2 && handler.Reports.All(r => r.Region == "TW" && r.GameVersion == build && r.Version == pluginVersion),
            "Login/session client identity was lost or plugin version changed.");
        using var unknownHttp = new HttpClient(handler, disposeHandler: false) { BaseAddress = http.BaseAddress };
        using var legacy = new CloudApiClient(unknownHttp);
        await legacy.ValidateSessionAsync("fixture-token", default);
        Check(handler.Reports[^1].Region == "TW" && handler.Reports[^1].GameVersion is null, "Missing game metadata blocked login or used a guessed build.");
        Console.WriteLine($"PASS cloud client identity: actual game-version file, unknown/malformed input, TW login/session headers and unchanged {pluginVersion}.");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private sealed class CaptureHandler : HttpMessageHandler
    {
        internal readonly List<(string? Region, string? GameVersion, string? Version)> Reports = [];
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            string? Header(string key) => request.Headers.TryGetValues(key, out var values) ? values.Single() : null;
            Reports.Add((Header("X-DACT-Region"), Header("X-DACT-Game-Version"), Header("X-DACT-Version")));
            if (request.Method == HttpMethod.Post)
            {
                using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(token));
                Check(body.RootElement.GetProperty("username").GetString() == "fixture" &&
                      body.RootElement.GetProperty("password").GetString() == "isolated-password" &&
                      body.RootElement.GetProperty("deviceId").GetString() == "fixture-device", "Identity reporting altered login credentials.");
            }
            else Check(request.Headers.Authorization?.ToString() == "Bearer fixture-token", "Session authentication changed.");
            return new(HttpStatusCode.OK) { Content = JsonContent.Create(new {
                token = "fixture-token", tokenType = "Bearer", expiresAt = DateTimeOffset.UtcNow.AddHours(1),
                user = new { id = "fixture", username = "fixture" },
            }) };
        }
    }
}
