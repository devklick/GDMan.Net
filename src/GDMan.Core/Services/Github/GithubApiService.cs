using System.Text.RegularExpressions;

using GDMan.Core.Helpers;
using GDMan.Core.Infrastructure;
using GDMan.Core.Models;
using GDMan.Core.Models.Github;

namespace GDMan.Core.Services.Github;

public class GithubApiService(WebApiService webApiService, ConsoleLogger logger)
{
    private readonly WebApiService _api = webApiService;
    private readonly ConsoleLogger _logger = logger;

    public GithubApiService(ConsoleLogger logger) : this(new WebApiService(new HttpClient
    {
        BaseAddress = new("https://api.github.com"),
        DefaultRequestHeaders =
        {
             {"User-Agent", "request"},
             {"Accept", "application/vnd.github+json"},
             {"X-GitHub-Api-Version", "2022-11-28"},
        }
    }), logger)
    { }

    public async Task<Result<IEnumerable<Release>>> GetReleasesAsync(string owner, string repo)
    {
        var result = await _api.GetAsync(
            GithubJsonContext.Default.IEnumerableRelease,
            GithubJsonContext.Default.Error,
            "repos",
            $"{owner}/{repo}",
            "releases");

        if (result.Error != null)
            result.Messages.Add(result.Error.Message);

        return new Result<IEnumerable<Release>>
        {
            Messages = result.Messages,
            Status = result.Status,
            Value = result.Value
        };
    }

    public async Task<Result<Release>> FindReleaseWithAssets(string owner, string repo, SemanticVersioning.Range? versionRange, IEnumerable<string> assetNameLike, bool latest)
    {
        var candidates = await FindReleases(owner, repo, versionRange);

        if (candidates.Status != ResultStatus.OK)
        {
            return new Result<Release>
            {
                Status = candidates.Status,
                Messages = candidates.Messages
            };
        }

        var release = candidates.Value!.OrderByDescending(r => r.PublishedAt).First();

        var assets = release.Assets.Where(a => assetNameLike.All(like => Regex.Match(a.Name, like).Success));

        if (!assets.Any())
        {
            return new Result<Release>
            {
                Status = ResultStatus.ClientError,
                Messages = [$"Found version {release.TagName} but no downloads match the specified criteria"]
            };
        }

        release.Assets = [assets.First()];

        return new Result<Release>
        {
            Status = ResultStatus.OK,
            Value = release
        };
    }

    private async Task<Result<IEnumerable<Release>>> FindReleases(string owner, string repo, SemanticVersioning.Range? versionRange)
    {
        var candidatesResult = await GetReleasesAsync(owner, repo);

        if (candidatesResult.Status != ResultStatus.OK) return candidatesResult;

        var candidates = candidatesResult.Value!;

        if (versionRange != null)
        {

            candidates = candidates.Where(c =>
                // Initially try to match excluding by ignoring stable pre-releases. 
                // This works when the user specified an exact version, e.g. 4.2.2, but tag is named 4.2.2-stable
                (SemVerHelper.TryParseVersion(c.TagName, ["stable"], out var version) && versionRange.IsSatisfied(version, true))
                // Fallback on matching with including stable pre-releases.
                // This works when the user includes the exact version including the stable pre-release, e.g. user specifies 4.2.2
                || (SemVerHelper.TryParseVersion(c.TagName, [], out var versionWithStable) && versionRange.IsSatisfied(versionWithStable, true)));
        }

        if (!candidates.Any())
        {
            return new Result<IEnumerable<Release>>
            {
                Status = ResultStatus.ClientError,
                Messages = ["Unable to find matching version"]
            };
        }

        return new Result<IEnumerable<Release>>
        {
            Value = candidates,
            Status = ResultStatus.OK,
        };
    }

}