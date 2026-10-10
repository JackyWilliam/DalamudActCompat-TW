internal sealed class IsolatedPluginFixture : IDisposable
{
    internal string Root { get; } = Path.Combine(Path.GetTempPath(), "DactHostFixture-" + Guid.NewGuid().ToString("N"));

    internal IsolatedPluginFixture(string source)
    {
        Directory.CreateDirectory(Root);
        try
        {
            foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(source, file);
                if (relative.StartsWith("matcha" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    continue;
                var target = Path.Combine(Root, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(file, target);
            }
            Console.WriteLine("Isolated ACT fixture: " + Root);
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        // Only the unique temp tree created above is owned by this fixture. On a
        // failed Host shutdown, retain it for diagnosis rather than killing processes.
        var temp = Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var root = Path.GetFullPath(Root);
        if (!root.StartsWith(temp, StringComparison.OrdinalIgnoreCase) ||
            !Path.GetFileName(root).StartsWith("DactHostFixture-", StringComparison.Ordinal))
            throw new InvalidOperationException("Fixture cleanup escaped its temporary root.");
        try { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); }
        catch (IOException) { Console.WriteLine("Retained locked test fixture: " + root); }
        catch (UnauthorizedAccessException) { Console.WriteLine("Retained inaccessible test fixture: " + root); }
    }
}
