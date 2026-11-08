using System.Net;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.Loader;
using System.Text;

// Loads the built plugin the way Dalamud's loader does with PreferSharedTypes off: an assembly the
// plugin's deps.json or folder provides is loaded privately, anything else falls back to the
// default context. On API 13 (.NET 9) the AI SDK brings System.Text.Json 10 into the plugin while
// framework assemblies such as System.Net.Http.Json stay bound to the in-box copy, so a JSON type
// handed across that line names two different types and fails with MissingMethodException. One
// test process shares a single copy, so only this split context reproduces what the game does.
internal static class PluginLoadContextSmokeTests
{
    internal static async Task RunAsync(string pluginDirectory)
    {
        var main = Path.Combine(pluginDirectory, "DalamudActCompat.dll");
        Check(File.Exists(main), $"Plugin build output is missing: {main}");
        Check(!ReferencesAssembly(main, "System.Net.Http.Json"),
            "DalamudActCompat.dll must not call System.Net.Http.Json: its JSON types are not the plugin's own on API 13.");

        var context = new DalamudLikeContext(main);
        try
        {
            var plugin = context.LoadFromAssemblyPath(main);
            if (File.Exists(Path.Combine(pluginDirectory, "System.Text.Json.dll")))
            {
                // The premise of this test: the packaged copy wins inside the plugin, as in Dalamud.
                var json = context.LoadFromAssemblyName(new AssemblyName("System.Text.Json"));
                Check(AssemblyLoadContext.GetLoadContext(json) == context,
                    "The packaged System.Text.Json was not loaded privately; the split this test guards is not reproduced.");
            }

            // Cloud sign-in: request body serialized and response read inside the plugin context.
            string? requestBody = null;
            using var http = new HttpClient(new Handler(async request =>
            {
                requestBody = await request.Content!.ReadAsStringAsync();
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """{"token":"fixture-token","tokenType":"Bearer","expiresAt":"2026-11-08T00:00:00Z","user":{"id":"u1","username":"fixture"},"keyEnvelope":null}""",
                        Encoding.UTF8, "application/json"),
                };
            })) { BaseAddress = new Uri("https://fixture.invalid/") };
            var apiType = plugin.GetType("DalamudActCompat.Infrastructure.Cloud.CloudApiClient", throwOnError: true)!;
            var api = Activator.CreateInstance(apiType, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null, [http, false, null], null)!;
            var login = (Task)apiType.GetMethod("LoginAsync")!.Invoke(api,
                ["fixture", "fixture-password-1", "device-1", CancellationToken.None, null, false])!;
            await login;
            var response = login.GetType().GetProperty("Result")!.GetValue(login)!;
            Check(response.GetType().GetProperty("Token")!.GetValue(response) as string == "fixture-token" &&
                  requestBody?.Contains("\"username\":\"fixture\"", StringComparison.Ordinal) == true,
                "Cloud sign-in JSON did not round-trip inside a Dalamud-like plugin context.");

            // AI assistant: the Claude client and its System.Text.Json 10 dependency load in the same context.
            var settingsType = plugin.GetType("DalamudActCompat.Ai.AiSettings", throwOnError: true)!;
            var settings = Activator.CreateInstance(settingsType)!;
            var claude = plugin.GetType("DalamudActCompat.Ai.AiClient", throwOnError: true)!
                .GetMethod("CreateClaudeClient", BindingFlags.Static | BindingFlags.NonPublic)!
                .Invoke(null, [settings, "fixture-key"]);
            Check(claude is not null && AssemblyLoadContext.GetLoadContext(claude.GetType().Assembly) == context,
                "The Anthropic client did not load inside the plugin context.");
            (claude as IDisposable)?.Dispose();
            (api as IDisposable)?.Dispose();
        }
        finally
        {
            context.Unload();
        }
        Console.WriteLine("Dalamud-like plugin load context: cloud JSON and AI client OK.");
    }

    private static bool ReferencesAssembly(string path, string name)
    {
        using var stream = File.OpenRead(path);
        using var pe = new PEReader(stream);
        var metadata = pe.GetMetadataReader();
        return metadata.AssemblyReferences.Any(handle =>
            metadata.GetString(metadata.GetAssemblyReference(handle).Name) == name);
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    // Mirrors Dalamud's ManagedLoadContext for plugins: deps.json first, then the plugin folder,
    // otherwise the default context (Dalamud itself, Lumina and the shared framework).
    private sealed class DalamudLikeContext(string mainAssembly)
        : AssemblyLoadContext("dact-plugin-load-context-test", isCollectible: true)
    {
        private readonly AssemblyDependencyResolver resolver = new(mainAssembly);
        private readonly string directory = Path.GetDirectoryName(mainAssembly)!;

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            if (assemblyName.Name is null or "Dalamud" or "Dalamud.Common" or "Lumina" or "Lumina.Excel") return null;
            var path = resolver.ResolveAssemblyToPath(assemblyName);
            if (path is not null && File.Exists(path)) return LoadFromAssemblyPath(path);
            var local = Path.Combine(directory, assemblyName.Name + ".dll");
            return File.Exists(local) ? LoadFromAssemblyPath(local) : null;
        }
    }

    private sealed class Handler(Func<HttpRequestMessage, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => send(request);
    }
}
