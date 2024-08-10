using System.Reflection;

using GDMan.Core.Infrastructure;
using GDMan.Core.Models;
using GDMan.Core.Services.Github;

using SharpCompress;

namespace GDMan.Core.Services;

public class UpdateCheckerService(GDManRepoService gdmanRepoService, ConsoleLogger logger)
{
    private readonly GDManRepoService _gdmanRepoService = gdmanRepoService;
    private readonly ConsoleLogger _logger = logger;


    public async Task CheckForUpdates()
    {
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
        }

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