using System.Runtime.Serialization;
using System.Text.RegularExpressions;

using GDMan.Core.Extensions;
using GDMan.Core.Models;
using GDMan.Core.Models.Github;

namespace GDMan.Core.Services.Github;

public class GithubApiService(WebApiService webApiService)
{
    private readonly WebApiService _api = webApiService;

    public GithubApiService() : this(new WebApiService(new HttpClient
    {
        BaseAddress = new("https://api.github.com"),
        DefaultRequestHeaders =
        {
             {"User-Agent", "request"},
             {"Accept", "application/vnd.github+json"},
             {"X-GitHub-Api-Version", "2022-11-28"},
        }
    }))
    { }

    public async Task<IEnumerable<Release>> GetReleasesAsync(string owner, string repo)
    {
        var result = await _api.GetAsync<List<Release>>("repos", $"{owner}/{repo}", "releases");

        if (result.Status == ResultStatus.OK) return result.Value!;

        throw new Exception("Request failed, handle later");
    }

    public async Task<Result<Release>> FindReleaseWithAsset(string owner, string repo, SemVer? semver, string[] assetNameLike, bool latest)
    {
        var candidates = await FindReleases(owner, repo, semver, latest);

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
            return null;
        }

        if (assets.Multiple())
        {
            return null;
        }

        release.Assets = [assets.First()];

        return new Result<Release>
        {
            Status = ResultStatus.OK,
            Value = release
        };
    }

    private async Task<Result<IEnumerable<Release>>> FindReleases(string owner, string repo, SemVer? semver, bool latest)
    {
        var candidates = await GetReleasesAsync(owner, repo);

        if (semver != null)
        {
            candidates = candidates.Where(r => SemVer.Parse(r.TagName).IsMatch(semver));
        }

        if (!candidates.Any())
        {
            return null;
        }

        if (candidates.Multiple() && !latest)
        {
            return null;
        }

        return new Result<IEnumerable<Release>>
        {
            Value = candidates,
            Status = ResultStatus.OK,
        };
    }

}