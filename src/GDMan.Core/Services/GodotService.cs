using GDMan.Core.Extensions;
using GDMan.Core.Infrastructure;
using GDMan.Core.Models;
using GDMan.Core.Models.Github;
using GDMan.Core.Services.FileSystem;
using GDMan.Core.Services.Github;

using Semver;

namespace GDMan.Core.Services;

public class GodotService(GithubApiService github, ConsoleLogger logger, GDManDirectory gdman)
{
    private readonly GithubApiService _gh = github;
    private readonly ConsoleLogger _logger = logger;
    private readonly GDManDirectory _gdman = gdman;

    public async Task<Result<object>> InstallAsync(
        SemVersionRange? versionRange, bool latest, Platform platform,
        Architecture architecture, Flavour flavour)
    {
        string versionName;
        GodotVersionDirectory? versionDir;


        // -------------------------------------------------
        // If exact version requested, check if that version is installed
        //      ? Yes, Update symlink to point to this version - done
        //      : No, proceed to download and install
        if (versionRange?.IsExactVersion(out var version) ?? false)
        {
            versionName = GDManDirectory.GenerateVersionName(version, platform, architecture, flavour);

            if (_gdman.GDManVersionsDirectory.AlreadyInstalled(versionName, out versionDir))
            {
                _logger.LogInformation($"Version {versionName} already installed, setting active");
                GDManDirectory.SetActive(versionDir);
                return new Result<object>(ResultStatus.OK, null);
            }
        }
        // -------------------------------------------------


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


        // -------------------------------------------------
        // Is the version found already installed?
        //      ? Yes, Update symlink to point to this version - done
        //      : No, proceed to download and install
        var release = ghResult.Value
            ?? throw new InvalidOperationException("Unable to find release");

        version = SemVersion.Parse(release.TagName, SemVersionStyles.Any);

        versionName = GDManDirectory.GenerateVersionName(version, platform, architecture, flavour);

        if (_gdman.GDManVersionsDirectory.AlreadyInstalled(versionName, out versionDir))
        {
            _logger.LogInformation($"Version {versionName} already installed, setting active");
            GDManDirectory.SetActive(versionDir);
            return new Result<object>(ResultStatus.OK, null);
        }
        // -------------------------------------------------


        // -------------------------------------------------
        // Download the requested version and set it as the active version
        var downloadUrl = release.Assets.SingleOrDefault()?.BrowserDownloadUrl
            ?? throw new InvalidOperationException("Unable to find exact version asset");

        versionDir = await _gdman.GDManVersionsDirectory.Install(downloadUrl, versionName);

        GDManDirectory.SetActive(versionDir);
        // -------------------------------------------------

        return new Result<object>();
    }

    public async Task<Result<IEnumerable<string>>> ListAsync()
    {
        var versions = await Task.FromResult(_gdman.GDManVersionsDirectory.List());

        return new Result<IEnumerable<string>>
        {
            Value = versions,
            Status = ResultStatus.OK,
        };
    }

    public async Task<Result<string>> GetCurrentVersion()
    {
        var current = await Task.FromResult(
            GDManDirectory.GodotLinkTargetVersion
            ?? "No active version");

        return new Result<string>
        {
            Status = ResultStatus.OK,
            Value = current
        };
    }

    private async Task<Result<Release>> FindDownloadAssetAsync(
        SemVersionRange? versionRange, bool latest, Platform platform,
        Architecture architecture, Flavour flavour)
    {
        var assetNameChecks = new List<string> { GDManDirectory.GenerateAssetName(platform, architecture, flavour) };

        if (flavour != Flavour.Mono) assetNameChecks.Add("^(?!.*mono).*$");

        var release = await _gh.FindReleaseWithAsset("godotengine", "godot", versionRange, assetNameChecks, latest);

        if (release.Status != ResultStatus.OK)
        {
            throw new Exception("Unable to search github for Godot releases. "
            + "Make sure you can access https://github.com/godotengine/godot/releases/"
            + Environment.NewLine + string.Join(Environment.NewLine, release.Messages));
        }

        _logger.LogInformation($"Found github release for {release.Value?.Assets.First().Name}");

        return release;
    }
}