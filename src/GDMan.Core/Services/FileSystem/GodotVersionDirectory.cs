using System.Runtime.Versioning;

using GDMan.Core.Exceptions;
using GDMan.Core.Extensions;
using GDMan.Core.Models;

using SharpCompress.Common;
using SharpCompress.Readers;

namespace GDMan.Core.Services.FileSystem;

/// <summary>
/// Class representing a directory that contains the files 
/// for a specific version of Godot, e.g. `gdman/versions/Godot_v1.2.3/`
/// </summary>
public class GodotVersionDirectory : IEquatable<GodotVersionDirectory>
{
    public string Path { get; }
    public string Name { get; }
    public string ExecutablePath => GetExecutablePath(Path);
    public string ZipPath => Directory.GetFiles(Path, "*.zip").Single();
    public SemanticVersioning.Version Version { get; }
    public SemanticVersioning.Version VersionWithoutStablePrerelease { get; }
    public Platform Platform { get; }
    public Architecture Architecture { get; }
    public Flavour Flavour { get; }
    public bool IsValid { get; } = true;

    public GodotVersionDirectory(string path)
    {
        Path = path;
        Name = System.IO.Path.GetFileName(path);
        Directory.CreateDirectory(path);
        Version = ParseSemverFromName(Name);
        VersionWithoutStablePrerelease = new(Version.ToString().TrimEnd("-stable"));
        Flavour = ParseFlavourFromName(Name);
        Platform = ParsePlatformFromName(Name);
        Architecture = ParseArchitectureFromName(Name, Platform);
    }

    public void ExtractZip()
    {
        var options = new ExtractionOptions
        {
            ExtractFullPath = true,
            Overwrite = true,
        };

        using (Stream stream = File.OpenRead(ZipPath))
        {
            using (var reader = ReaderFactory.Open(stream))
            {
                while (reader.MoveToNextEntry())
                {
                    if (reader.Entry.IsDirectory) continue;

                    reader.WriteEntryToDirectory(Path, options);
                }
            }
        }

        // Delete the zip
        File.Delete(ZipPath);
    }

    public void Delete()
        => Directory.Delete(Path, true);

    public void CleanStructure()
    {
        // In some cases, the contents of the zip have a folder with the same name, 
        // then the main contents nested within that, so when extracted we end up 
        // having to folders with the same name, eg:
        // some-name/
        //      some-name/
        //          file1
        // We dont want this, we want the main files to be at the top level of the folder
        var parentDir = new DirectoryInfo(Path).Parent!.FullName;
        var doubleFolderPath = System.IO.Path.Combine(parentDir, Name, Name);
        if (Directory.Exists(doubleFolderPath))
        {
            var index = doubleFolderPath.LastIndexOf(Name);
            foreach (var entry in Directory.GetFileSystemEntries(doubleFolderPath))
            {
                var newPath = entry.Remove(index, Name.Length);

                if (File.Exists(entry))
                {
                    new FileInfo(entry).MoveTo(newPath);
                }
                else
                {
                    new DirectoryInfo(entry).MoveTo(newPath);
                }
            }

            Directory.Delete(doubleFolderPath);
        }
    }

    public void EnsureExecutable()
    {
        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            EnsureExecutableOnUnix();
        }
    }

    [SupportedOSPlatform("linux"), SupportedOSPlatform("macos")]
    private void EnsureExecutableOnUnix()
    {
        // Now we need to make the file executable. 
        // Ideally we'd preserve the file attributes when extracting, 
        // but this doesnt seem to be implemented in sharpcompress

        File.SetUnixFileMode(ExecutablePath, File.GetUnixFileMode(ExecutablePath) | UnixFileMode.UserExecute);
    }

    /// <summary>
    /// Checks the specified path, which is expected to be a folder, 
    /// and finds the executable file within in, returning the path to that file.
    /// </summary>
    /// <param name="versionDir">The path to the folder containing the files 
    /// for the relevant version of Godot</param>
    /// <exception cref="FileNotFoundException"></exception> 
    /// <returns>The path to the executable file, if found</returns>
    private static string GetExecutablePath(string path)
    {
        var dir = new DirectoryInfo(path);
        var fileName = dir.Name;

        var files = dir.GetFiles();

        if (files.Length == 1) return files.First().FullName;

        foreach (var file in files)
        {
            if (file.Name == fileName || System.IO.Path.GetFileNameWithoutExtension(file.Name) == fileName)
            {
                return file.FullName;
            }
        }

        throw new FileNotFoundException($"Cannot find executable file in version directory {path}");
    }

    public bool HasExecutable()
    {
        try
        {
            GetExecutablePath(Path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool Equals(GodotVersionDirectory? other)
        => other != null && other.Path == Path;

    private static SemanticVersioning.Version ParseSemverFromName(string name)
    {
        // The semver string is between the first and second underscores
        var startPos = name.IndexOf('_');
        var endPos = name.IndexOf('_', startPos + 1);

        if (startPos < 0 || endPos < 0)
        {
            throw new InvalidSemVerException(
                $"Version directory {name} does not match the expected format. " +
                "Unable to determine the version of Godot this folder relates to");
        }

        // We +2 to exclude the leading underscore and 'v';
        // We -2 to exclude the trailing underscore, and because we shifted the start position
        var semverString = name.Substring(startPos + 2, endPos - startPos - 2);

        return SemanticVersioning.Version.Parse(semverString);
    }

    private static Architecture ParseArchitectureFromName(string name, Platform platform)
    {
        if (platform == Platform.MacOS) return Architecture.Universal;
        if (platform == Platform.Windows)
        {
            if (name.Contains("win32", StringComparison.CurrentCultureIgnoreCase)) return Architecture.X86;
            if (name.Contains("win64", StringComparison.CurrentCultureIgnoreCase)) return Architecture.X64;
        }
        if (platform == Platform.Linux)
        {
            if (name.Contains("arm32", StringComparison.CurrentCultureIgnoreCase)) return Architecture.Arm32;
            if (name.Contains("arm64", StringComparison.CurrentCultureIgnoreCase)) return Architecture.Arm64;
            if (name.Contains("x86_64", StringComparison.CurrentCultureIgnoreCase)) return Architecture.X64;
            if (name.Contains("x86_32", StringComparison.CurrentCultureIgnoreCase)) return Architecture.X86;
        }

        throw new InvalidSemVerException(
            $"Version directory {name} does not match the expected format. " +
            "Unable to determine the system architecture that this folder relates to");
    }

    private static Platform ParsePlatformFromName(string name)
    {
        if (name.Contains("win32", StringComparison.CurrentCultureIgnoreCase) || name.Contains("win64", StringComparison.CurrentCultureIgnoreCase))
            return Platform.Windows;

        if (name.Contains("macos", StringComparison.CurrentCultureIgnoreCase))
            return Platform.MacOS;

        if (name.Contains("linux", StringComparison.CurrentCultureIgnoreCase))
            return Platform.Linux;

        throw new InvalidSemVerException(
            $"Version directory {name} does not match the expected format. " +
            "Unable to determine the platform that this folder relates to");
    }

    private static Flavour ParseFlavourFromName(string name)
        => name.Contains("mono", StringComparison.CurrentCultureIgnoreCase) ? Flavour.Mono : Flavour.Standard;
}