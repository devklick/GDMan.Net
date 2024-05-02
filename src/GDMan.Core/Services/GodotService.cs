using GDMan.Core.Extensions;
using GDMan.Core.Infrastructure;
using GDMan.Core.Models;
using GDMan.Core.Models.Github;
using GDMan.Core.Services.FileSystem;
using GDMan.Core.Services.Github;

using SemanticVersioning;

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
        if (versionRange?.IsExactVersion(out var version) ?? false)
        {
            versionName = _gdman.GenerateVersionName(version, platform, architecture, flavour);

            if (_gdman.GDManVersionsDirectory.AlreadyInstalled(versionName, out versionDir))
            {
                _logger.LogInformation($"Version {versionName} already installed, setting active");
                _gdman.SetActive(versionDir);
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

        version = SemanticVersioning.Version.Parse(release.TagName);

        versionName = _gdman.GenerateVersionName(version, platform, architecture, flavour);

        if (_gdman.GDManVersionsDirectory.AlreadyInstalled(versionName, out versionDir))
        {
            _logger.LogInformation($"Version {versionName} already installed, setting active");
            _gdman.SetActive(versionDir);
            return new Result<object>(ResultStatus.OK, null);
        }
        // -------------------------------------------------


        // -------------------------------------------------
        // Download the requested version and set it as the active version
        var downloadUrl = release.Assets.SingleOrDefault()?.BrowserDownloadUrl
            ?? throw new InvalidOperationException("Unable to find exact version asset");

        versionDir = await _gdman.GDManVersionsDirectory.Install(downloadUrl, versionName);

        _gdman.SetActive(versionDir);
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

    public async Task<Result<object>> UninstallAsync(SemanticVersioning.Range? versionRange, Platform? platform, Architecture? architecture, Flavour? flavour, bool force, bool unused)
    {
        var remove = new List<GodotVersionDirectory>();

        // find the version(s) that the parameters point to
        foreach (var godotVersion in _gdman.GDManVersionsDirectory.List())
        {
            if (unused && godotVersion != _gdman.GodotCurrentVersion)
            {
                remove.Add(godotVersion);
            }
            else if (versionRange != null && versionRange.IsSatisfied(godotVersion.Version))
            {
                if (platform.HasValue && platform.Value != godotVersion.Platform) continue;
                if (architecture.HasValue && architecture.Value != godotVersion.Architecture) continue;
                if (flavour.HasValue && flavour.Value != godotVersion.Flavour) continue;

                remove.Add(godotVersion);
            }
        }

        if (remove.Count == 0)
            return new Result<object> { Status = ResultStatus.OK };

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
            if (version == _gdman.GodotCurrentVersion)
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
            version.Delete();
        }


        return await Task.FromResult(new Result<object> { Status = ResultStatus.OK });
    }
}