using System.Net;

using GDMan.Core.Extensions;
using GDMan.Core.Models;
using GDMan.Core.Models.Github;
using GDMan.Core.Services.Github;

using Semver;

namespace GDMan.Core.Services;

public class GodotService(GithubApiService github, FileSystemService fs)
{
    private readonly GithubApiService _gh = github;
    private readonly FileSystemService _fs = fs;


    public async Task<Result<object>> ProcessAsync(
        SemVersionRange? versionRange, bool latest, Platform platform,
        Architecture architecture, Flavour flavour)
    {
        // If exact version requested, check if that version is installed
        //      ? Yes, Update symlink to point to this version - done
        //      : No, proceed to download and install
        if (_fs.AlreadyInstalled(versionRange, platform, architecture, flavour, out var dir))
        {
            _fs.SetActive(dir);
            return new Result<object>(ResultStatus.OK, null);
        }
        // -------------------------------------------------

        // Find matching version via github
        //    ? Found, proceed to download/installation
        //    : Not found, report error to user
        var ghResult = await FindDownloadAssetAsync(versionRange, latest, platform, architecture, flavour);
        if (ghResult.Status != ResultStatus.OK)
        {
            return new Result<object>
            {
                Status = ghResult.Status,
                Messages = ghResult.Messages
            };
        }
        // -------------------------------------------------

        // Is the version found already installed?
        //      ? Yes, Update symlink to point to this version - done
        //      : No, proceed to download and install
        var release = ghResult.Value
            ?? throw new InvalidOperationException("Unable to find release");

        var version = SemVersion.Parse(release.TagName, SemVersionStyles.Any);

        if (_fs.AlreadyInstalled(version, platform, architecture, flavour, out dir))
        {
            _fs.SetActive(dir);
            return new Result<object>(ResultStatus.OK, null);
        }
        // -------------------------------------------------

        // Download to versions folder
        var downloadUrl = release.Assets.SingleOrDefault()?.BrowserDownloadUrl
            ?? throw new InvalidOperationException("Unable to find exact version asset");

        var zip = await _fs.DownloadGodotVersion(downloadUrl);
        // -------------------------------------------------

        // Extract to versions folder & delete zip
        var newVersionDir = _fs.ExtractGodotVersion(zip);
        // -------------------------------------------------

        // Update symlink to point to new version
        _fs.SetActive(newVersionDir);
        // -------------------------------------------------


        return new Result<object>();
    }

    public async Task<Result<Release>> FindDownloadAssetAsync(
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
        return [$"{GetFlavourPrefix(flavour)}linux_{architecture.ToString()?.ToLower()}"];
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