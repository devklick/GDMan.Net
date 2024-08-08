using GDMan.Core.Extensions;
using GDMan.Core.Helpers;
using GDMan.Core.Infrastructure;
using GDMan.Core.Models;
using GDMan.Core.Models.Github;
using GDMan.Core.Services.FileSystem;
using GDMan.Core.Services.Github;

namespace GDMan.Core.Services;

public class GodotService(GithubApiService github, ConsoleLogger logger, GDManDirectory gdman)
{
    private readonly GithubApiService _gh = github;
    private readonly ConsoleLogger _logger = logger;
    private readonly GDManDirectory _gdman = gdman;

    public async Task<Result<object>> InstallAsync(
        SemanticVersioning.Range? versionRange, bool latest, Platform platform,
        Architecture architecture, Flavour flavour)
    {
        string versionName;
        GodotVersionDirectory? versionDir;


        // -------------------------------------------------
        // If exact version requested, check if that version is installed
        //      ? Yes, Update symlink to point to this version - done
        //      : No, proceed to download and install
        // The version is saved to disk including the stable pre-release flag in the name, if that's how it's published on github. 
        // So here, we dont want to exclude the stable pre-release flag when parsing
        if (versionRange != null && SemVerHelper.TryParseVersion(versionRange.ToString(), [], out var version))
        {
            versionName = _gdman.GenerateVersionName(version, platform, architecture, flavour);

            if (_gdman.GDManVersionsDirectory.AlreadyInstalled(versionName, out versionDir))
            {
                _logger.LogInformation($"Version {versionName} already installed, setting active");
                await _gdman.SetActive(versionDir);
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

        // The version is saved to disk including the stable pre-release flag in the name, if that's how it's published on github. 
        // So here, we dont want to exclude the stable pre-release flag when parsing
        if (!SemVerHelper.TryParseVersion(release.TagName, [], out version))
        {
            throw new InvalidOperationException($"Release {release.TagName} has an invalid version number");
        }

        versionName = _gdman.GenerateVersionName(version, platform, architecture, flavour);

        if (_gdman.GDManVersionsDirectory.AlreadyInstalled(versionName, out versionDir))
        {
            _logger.LogInformation($"Version {versionName} already installed, setting active");
            await _gdman.SetActive(versionDir);
            return new Result<object>(ResultStatus.OK, null);
        }
        // -------------------------------------------------


        // -------------------------------------------------
        // Download the requested version and set it as the active version
        var downloadUrl = release.Assets.SingleOrDefault()?.BrowserDownloadUrl
            ?? throw new InvalidOperationException("Unable to find exact version asset");

        versionDir = await _gdman.GDManVersionsDirectory.Install(downloadUrl, versionName);

        await _gdman.SetActive(versionDir);
        // -------------------------------------------------

        return new Result<object>();
    }

    public async Task<Result<IEnumerable<string>>> ListAsync()
    {
        var versions = await Task.FromResult(_gdman.GDManVersionsDirectory.List());

        foreach (var version in versions)
        {
            _logger.LogInformation(version.Name);
        }

        return new Result<IEnumerable<string>>
        {
            Value = versions.Select(v => v.Name),
            Status = ResultStatus.OK,
        };
    }

    public async Task<Result<string>> GetCurrentVersion()
    {
        var current = await Task.FromResult(
            _gdman.GodotCurrentVersion?.Name
            ?? "No active version");

        _logger.LogInformation(current);

        return new Result<string>
        {
            Status = ResultStatus.OK,
            Value = current
        };
    }

    private async Task<Result<Release>> FindDownloadAssetAsync(
        SemanticVersioning.Range? versionRange, bool latest, Platform platform,
        Architecture architecture, Flavour flavour)
    {
        var assetNameChecks = new List<string> { _gdman.GenerateAssetName(platform, architecture, flavour) };

        if (flavour != Flavour.Mono) assetNameChecks.Add("^(?!.*mono).*$");

        var release = await _gh.FindReleaseWithAssets("godotengine", "godot", versionRange, assetNameChecks, latest);

        if (release.Status != ResultStatus.OK)
        {
            throw new Exception("Unable to search github for Godot releases. "
            + "Make sure you can access https://github.com/godotengine/godot/releases/"
            + Environment.NewLine + string.Join(Environment.NewLine, release.Messages));
        }

        if (release.Value?.Assets.Multiple() ?? false)
        {
            return new Result<Release>
            {
                Status = ResultStatus.ClientError,
                Messages = [$"Found release {release.Value.Name} containing multiple assets matching the specified criteria"]
            };
        }

        _logger.LogInformation($"Found github release for {release.Value?.Assets.First().Name}");

        return release;
    }

    public async Task<Result<object>> UninstallAsync(SemanticVersioning.Range? versionRange, Platform? platform, Architecture? architecture, Flavour? flavour, bool force, bool unused)
    {
        var remove = new List<GodotVersionDirectory>();

        // find the version(s) that the parameters point to
        foreach (var godotVersion in _gdman.GDManVersionsDirectory.List())
        {
            if (unused && !godotVersion.Equals(_gdman.GodotCurrentVersion))
            {
                remove.Add(godotVersion);
            }
            else if (versionRange != null && versionRange.IsSatisfied(godotVersion.VersionWithoutStablePrerelease))
            {
                if (platform.HasValue && platform.Value != godotVersion.Platform) continue;
                if (architecture.HasValue && architecture.Value != godotVersion.Architecture) continue;
                if (flavour.HasValue && flavour.Value != godotVersion.Flavour) continue;

                remove.Add(godotVersion);
            }
        }

        if (remove.Count == 0)
        {
            _logger.LogInformation("No versions found to uninstall");
            return new Result<object> { Status = ResultStatus.OK };
        }

        _logger.LogInformation("Found the following versions to uninstall");

        remove.ForEach(version => _logger.LogInformation(version.Name));

        // If multiple found and not forced/unused, log that --force is required
        if (remove.Multiple() && !force && !unused)
        {
            return new Result<object>
            {
                Status = ResultStatus.ClientError,
                Messages = ["Multiple versions can only be uninstalled when using the --force or --unused options"]
            };
        }

        foreach (var version in remove)
        {
            // If the version is the, log that the current version needs to be changed first
            if (version.Equals(_gdman.GodotCurrentVersion))
            {
                return new Result<object>
                {
                    Status = ResultStatus.ClientError,
                    Messages = [
                        $"Cannot uninstall {version.Name} because it is currently active",
                        "Install / enable another version before uninstalling this"
                    ]
                };
            }

            // remove the version
            _logger.LogInformation($"Removing {version.Name}");
            version.Delete();
        }


        return await Task.FromResult(new Result<object> { Status = ResultStatus.OK });
    }
}