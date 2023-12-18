using GDMan.Core.Models;
using GDMan.Core.Services.Github;

namespace GDMan.Core.Services;

public class GodotService(GithubApiService github, FileSystemService fileSystemService)
{
    private readonly GithubApiService _gh = github;
    private readonly FileSystemService _fs = fileSystemService;

    public async Task<Result<string>> FindDownloadAssetAsync(
        string version, bool latest, Platform platform,
        Architecture architecture, Flavour flavour)
    {
        await _gh.FindReleaseWithAsset("godotengine", "godot", "4.2.1", ["linux.arm32"], true);

        throw new Exception();
    }
    /*
        - find asset using cli args
        - Download asset to relevant location
        - Unzip asset
        - Create/update latest symlink
    */
}