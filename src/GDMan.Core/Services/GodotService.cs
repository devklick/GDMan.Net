using GDMan.Core.Models;
using GDMan.Core.Services.Github;

using Semver;

namespace GDMan.Core.Services;

public class GodotService(GithubApiService github, FileSystemService fileSystemService)
{
    private readonly GithubApiService _gh = github;
    private readonly FileSystemService _fs = fileSystemService;

    public async Task<Result<string>> FindDownloadAssetAsync(
        SemVersionRange? versionRange, bool latest, Platform platform,
        Architecture architecture, Flavour flavour)
    {
        var assetNameChecks = GetAssetNameChecks(platform, architecture, flavour);
        var release = await _gh.FindReleaseWithAsset("godotengine", "godot", versionRange, assetNameChecks, latest);

        if (release.Status != ResultStatus.OK) throw new Exception("TO DO");

        throw new Exception();
    }

    private static IEnumerable<string> GetAssetNameChecks(Platform platform, Architecture architecture, Flavour flavour)
    {
        if (platform == Platform.Windows) return GetWindowsAssetNameChecks(architecture, flavour);
        if (platform == Platform.MacOS) return GetMacOSAssetNameChecks(architecture, flavour);
        if (platform == Platform.Linux) return GetLinuxAssetNameChecks(architecture, flavour);

        throw new NotImplementedException($"Unsupported architecture {architecture}");
    }

    private static IEnumerable<string> GetLinuxAssetNameChecks(Architecture architecture, Flavour flavour)
    {
        /*
        Possible options are:
            linux_arm32
            linux_arm64
            linux_x86_32
            linux_x86_64
            mono_linux_arm32
            mono_linux_arm64
            mono_linux_x86_32
            mono_linux_x86_64
        */

        // TODO: This mapping needs work
        return [$"{GetFlavourPrefix(flavour)}linux_{architecture.Identifier.ToLower()}"];
    }

    private static IEnumerable<string> GetWindowsAssetNameChecks(Architecture architecture, Flavour flavour)
    {
        /*
        Possible options are:
            win32
            win64
            mono_win32
            mono_win64
        */
        // TODO: This mapping probably needs work as well
        return [$"{GetFlavourPrefix(flavour)}win{(architecture == Architecture.Arm32 || architecture == Architecture.X86 ? "32" : "64")}"];
    }

    private static IEnumerable<string> GetMacOSAssetNameChecks(Architecture architecture, Flavour flavour)
    {
        /*
        Possible options are:
            macos.universal
            mono_macos.universal
        */
        return [$"{GetFlavourPrefix(flavour)}macos.universal"];
    }

    private static string GetFlavourPrefix(Flavour flavour)
        => flavour == Flavour.Mono ? "mono_" : "";
}