using System.Diagnostics.CodeAnalysis;

using GDMan.Core.Config;

namespace GDMan.Core.Services.FileSystem;

/// <summary>
/// Class representing the directory where Godot versions are installed. 
/// E.g. <c>.gdman/versions</c>
/// </summary>
public class GDManVersionsDirectory(string gdmanDir, HttpClient? client = null)
{
    public string Path { get; } = EnvVars.VersionsDir.Value ?? System.IO.Path.Join(gdmanDir, "versions");
    private readonly HttpClient _client = client ?? new HttpClient();

    public async Task<GodotVersionDirectory> Install(string url, string destinationFolderName)
    {
        var dir = new GodotVersionDirectory(System.IO.Path.Join(Path, destinationFolderName));

        await Download(url, dir);

        dir.ExtractZip();

        dir.CleanStructure();

        dir.EnsureExecutable();

        return dir;
    }

    public bool AlreadyInstalled(string versionName, [NotNullWhen(true)] out GodotVersionDirectory? directory)
    {
        var path = System.IO.Path.Join(Path, versionName);

        if (Directory.Exists(path))
        {
            directory = new GodotVersionDirectory(path);
            return true;
        }

        directory = null;
        return false;
    }

    private async Task<string> Download(string url, GodotVersionDirectory targetDir)
    {
        var zip = System.IO.Path.Join(targetDir.Path, $"{targetDir.Name}.zip");
        var response = await _client.GetAsync(url);

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
            throw new Exception("Unable to download file");

        using FileStream fs = File.Open(zip, FileMode.Create);
        await response.Content.CopyToAsync(fs);

        return zip;
    }
}