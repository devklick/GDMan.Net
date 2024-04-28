using System.Diagnostics.CodeAnalysis;

using GDMan.Core.Infrastructure;

namespace GDMan.Core.Services.FileSystem;

/// <summary>
/// Class representing the directory where Godot versions are installed. 
/// E.g. <c>gdman/versions</c>
/// </summary>
public class GDManVersionsDirectory(ConsoleLogger logger, HttpClient? client = null)
{
    public string Path { get; } = KnownPaths.GDManVersionsPath;

    private readonly ConsoleLogger _logger = logger;
    private readonly HttpClient _client = client ?? new HttpClient();

    public async Task<GodotVersionDirectory> Install(string url, string destinationFolderName)
    {
        var dir = new GodotVersionDirectory(System.IO.Path.Join(Path, destinationFolderName));

        await Download(url, dir);

        _logger.LogInformation($"Extracting contents to {dir.Path}");

        dir.ExtractZip();

        _logger.LogInformation($"Tidying zip output {dir.Path}");

        dir.CleanStructure();

        _logger.LogInformation($"Making executable");

        dir.EnsureExecutable();

        return dir;
    }

    public IEnumerable<string> List()
        => Directory.GetDirectories(Path).Select(p => new DirectoryInfo(p).Name)!;

    public bool AlreadyInstalled(string versionName, [NotNullWhen(true)] out GodotVersionDirectory? directory)
    {
        var path = System.IO.Path.Join(Path, versionName);

        if (Directory.Exists(path))
        {
            directory = new GodotVersionDirectory(path);

            if (directory.HasExecutable()) return true;
        }

        directory = null;
        return false;
    }

    private async Task<string> Download(string url, GodotVersionDirectory targetDir)
    {
        _logger.LogInformation($"Downloading {url} to {targetDir}");
        var zip = System.IO.Path.Join(targetDir.Path, $"{targetDir.Name}.zip");
        var response = await _client.GetAsync(url);

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
            throw new Exception("Unable to download file");

        using FileStream fs = File.Open(zip, FileMode.Create);
        await response.Content.CopyToAsync(fs);

        return zip;
    }
}