using System.Reflection;

using GDMan.Core.Config;
using GDMan.Core.Infrastructure;
using GDMan.Core.Models;
using GDMan.Core.Services.Github;

using SharpCompress;

namespace GDMan.Core.Services;

public class UpdateCheckerService(GDManRepoService gdmanRepoService, ConsoleLogger logger)
{
    private readonly GDManRepoService _gdmanRepoService = gdmanRepoService;
    private readonly ConsoleLogger _logger = logger;

    /// <summary>
    /// Whether or not we have checked for updates during the current process.
    /// </summary>
    public bool Checked { get; private set; }


    public async Task CheckForUpdates()
    {
        // The check may have already happened on schedule
        // and we may now be trying to perform the check on user request. 
        // Skip if we've already performed the check in the current process.
        if (Checked) return;

        var result = await _gdmanRepoService.GetLatestVersion();

        if (result.Status != ResultStatus.OK || result.Value == null)
        {
            result.Messages.ForEach(_logger.LogError);
            result.Error?.ForEach(_logger.LogError);
            _logger.NewLine();
            return;
        }
        var currentVersion = GetCurrentVersion();

        if (currentVersion == null)
        {
            _logger.LogError("Cant determine current version of GDMan. Unable to check for updates");
            return;
        }

        // DOESNT WORK ON LINUX - env vars only persisted in current process.
        // TODO: use config file instead
        Environment.SetEnvironmentVariable(EnvVars.LastCheckedForUpdates.Name, DateTime.UtcNow.ToString(), EnvironmentVariableTarget.User);
        Checked = true;

        var latest = result.Value;

        if (latest.Version > currentVersion)
        {
            _logger.LogInformation($"Version {latest.Version} is available!");
            _logger.LogInformation($"To see changelog, visit {latest.ReleaseUrl}");
            GetUpdateInstructions().ForEach(_logger.LogInformation);
            _logger.NewLine();
            return;
        }

        _logger.LogInformation("Already on latest version");
    }

    public async Task CheckForUpdatesIfDue()
    {
        if (AutoUpdateCheckDue())
        {
            await CheckForUpdates();
        }
    }

    private bool AutoUpdateCheckDue()
    {
        // temporarily disable auto checking until we can persist
        // the last check date in config file so it's cross-platform
        return false;

        // var envVar = EnvVars.LastCheckedForUpdates.Value;
        // if (string.IsNullOrEmpty(envVar)) return true;
        // if (!DateTime.TryParse(EnvVars.LastCheckedForUpdates.Value, out var lastChecked)) return true;
        // // Ideally should make the frequency configurable. Maybe in a future version...
        // if (lastChecked.AddDays(7) < DateTime.UtcNow) return true;
        // return false;
    }

    private static SemanticVersioning.Version? GetCurrentVersion()
    {
        var versionString = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

        return string.IsNullOrEmpty(versionString)
            ? null
            : SemanticVersioning.Version.Parse(versionString, loose: true);
    }

    private static string[] GetUpdateInstructions()
    {
        if (OperatingSystem.IsWindows())
        {
            return [
                "Run the following in PowerShell to update:",
                ". {iwr -useb https://raw.githubusercontent.com/devklick/GDMan/master/install/install-windows.ps1} | iex;"
            ];
        }
        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            var os = OperatingSystem.IsLinux() ? "linux" : "osx";
            return [
                "Run the following to update:",
                $"wget -q https://raw.githubusercontent.com/devklick/GDMan/master/install/install-unix.sh -O - | bash -s {os}"
            ];
        }
        return [
            "You can manually download and install the correct version for your operating system from here"
        ];
    }
}