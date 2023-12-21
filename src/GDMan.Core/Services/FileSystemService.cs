using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Text;

using GDMan.Core.Extensions;
using GDMan.Core.Models;

using Semver;

using SharpCompress.Common;
using SharpCompress.Readers;

namespace GDMan.Core.Services;

/*
Structure:
    .gdman/
        bin/
            godot <- symlink to a specific version
            gdman <- symlink to this apps executable
        versions/ <- The godot versions that have been downloaded
            godot_1.2.3-stable/
            godot_3.4.5-alpha/
        GDMan/
            ...contents of this app
*/

public class FileSystemService(HttpClient downloadClient)
{
    public static readonly string GDManDirectory = Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        ".gdman"
    );

    public static readonly string GDManVersionDirectory =
        Environment.GetEnvironmentVariable("GDMAN_VERSIONS_DIR")
        ?? Path.Join(GDManDirectory, "versions");

    public static readonly string GDManBinDirectory = Path.Join(
        GDManDirectory,
        "bin"
    );

    private static readonly string GodotLinkPath = Path.Combine(GDManBinDirectory, "godot");

    private readonly HttpClient _client = downloadClient;


    public bool AlreadyInstalled(SemVersion version, Platform platform, Architecture architecture, Flavour flavour, [NotNullWhen(true)] out string? directory)
    {
        var name = GenerateName(version, platform, architecture, flavour);

        directory = Path.Join(GDManVersionDirectory, name);
        if (Directory.Exists(directory))
        {
            return true;
        }

        directory = null;
        return false;
    }

    public bool AlreadyInstalled(SemVersionRange? range, Platform platform, Architecture architecture, Flavour flavour, [NotNullWhen(true)] out string? directory)
    {
        directory = null;
        if (range == null) return false;

        return range.IsExactVersion(out var version)
            && AlreadyInstalled(version, platform, architecture, flavour, out directory);
    }

    public void SetActive(string versionDir)
    {
        // Find the executable within the version directory
        var exeFile = GetExecutablePathForVersion(versionDir);

        // Delete the current symlink if it exists
        if (File.Exists(GodotLinkPath)) File.Delete(GodotLinkPath);

        // Create the symlink pointing to the exe file
        File.CreateSymbolicLink(GodotLinkPath, exeFile);
    }

    public async Task<string> DownloadGodotVersion(string url)
    {
        var destination = Path.Join(GDManVersionDirectory, Path.GetFileName(url));

        var response = await _client.GetAsync(url);

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
            throw new Exception("Unable to download file");

        using FileStream fs = File.Open(destination, FileMode.CreateNew);
        await response.Content.CopyToAsync(fs);

        return destination;
    }

    public string ExtractGodotVersion(string path)
    {
        var parentDir = Path.GetDirectoryName(path)
            ?? throw new Exception($"Unable to get parent directory for path {path}");

        var folderName = Path.GetFileNameWithoutExtension(path);
        var destination = Path.Combine(parentDir, folderName);

        using (Stream stream = File.OpenRead(path))
        using (var reader = ReaderFactory.Open(stream))
        {
            while (reader.MoveToNextEntry())
            {
                if (!reader.Entry.IsDirectory)
                {
                    Console.WriteLine(reader.Entry.Key);
                    reader.WriteEntryToDirectory(destination, new ExtractionOptions
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                }
            }
        }

        // In some cases, the contents of the zip have a folder with the same name, 
        // then the main contents nested within that, so when extracted we end up 
        // having to folders with the same name, eg:
        // some-name/
        //      some-name/
        //          file1
        // We dont want this, we want the main files to be at the top level of the folder
        var doubleFolderPath = Path.Combine(parentDir, folderName, folderName);
        if (Directory.Exists(doubleFolderPath))
        {
            var index = doubleFolderPath.LastIndexOf(folderName);
            foreach (var entry in Directory.GetFileSystemEntries(doubleFolderPath))
            {
                var newPath = entry.Remove(index, folderName.Length);

                if (File.Exists(entry))
                {
                    new FileInfo(entry).MoveTo(newPath);
                }
                else
                {
                    new DirectoryInfo(entry).MoveTo(newPath);
                }
            }
        }

        return destination;
    }

    public static string GenerateName(SemVersion version, Platform platform, Architecture architecture, Flavour flavour)
    {
        var sb = new StringBuilder();
        sb.Append("Godot_v");
        sb.Append(version.ToString());
        sb.Append('_');

        if (flavour == Flavour.Mono) sb.Append("mono_");

        if (platform == Platform.Windows)
        {
            sb.Append("win");
            if (architecture == Architecture.X64) sb.Append("64");
            else if (architecture == Architecture.X86) sb.Append("32");
            else throw new InvalidOperationException($"Architecture {architecture} not supported on {platform} platform");

            if (flavour != Flavour.Mono) sb.Append(".exe");
        }
        else if (platform == Platform.Linux)
        {
            sb.Append("linux");
            sb.Append(flavour == Flavour.Mono ? '_' : '.');

            // TODO: This will almost definitely need work
            if (architecture == Architecture.Arm32) sb.Append("arm32");
            else if (architecture == Architecture.Arm64) sb.Append("arm64");
            else if (architecture == Architecture.X64) sb.Append("x86_64");
            else if (architecture == Architecture.X86) sb.Append("x86_32");
        }
        else if (platform == Platform.MacOS)
        {
            sb.Append("macos.universal");
        }
        else throw new InvalidOperationException("Unsupported platform");

        return sb.Append(".zip").ToString();
    }

    private string GetExecutablePathForVersion(string versionDir)
    {
        // The executable is expected to be a direct child of the versionDir, 
        // and is expected to have the exact same name
        // or the same name excluding file extension

        var dir = new DirectoryInfo(versionDir);
        var fileName = dir.Name;

        foreach (var file in dir.GetFiles())
        {
            if (file.FullName == fileName || Path.GetFileNameWithoutExtension(file.FullName) == fileName)
            {
                return file.FullName;
            }
        }

        throw new FileNotFoundException($"Cannot find executable file in version directory {versionDir}");
    }

    private string GetFileNameWithoutExtension(string path, int maxDistanceFromEnd = 4)
    {
        var dotIndex = path.IndexOf('.', path.Length - maxDistanceFromEnd);

        return dotIndex > 0 ? path.Substring(0, dotIndex) : path;
    }
}