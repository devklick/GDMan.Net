using GDMan.Core.Infrastructure;
using GDMan.Core.Models;

namespace GDMan.Core.Services.Github;

public class GDManRepoService(GithubApiService githubApiService, ConsoleLogger logger)
{
    private readonly GithubApiService _githubApiService = githubApiService;
    private readonly ConsoleLogger _logger = logger;

    public async Task<Result<GDManVersionInfo>> GetLatestVersion()
    {
        var result = await _githubApiService.GetReleasesAsync("devklick", "gdman");

        if (result.Status != ResultStatus.OK || result.Value == null)
        {
            return new Result<GDManVersionInfo>
            {
                Error = ["Error checking for updates"],
                Messages = result.Messages,
                Status = ResultStatus.ServerError,
            };
        }

        // We should be able to assume the first item in the array is the latest release, 
        // however lets order them to make sure. This increases confidence but also increases
        // execution time as more and more versions are published. Might want to address this in the future.
        var latest = result.Value.OrderByDescending(r => r.PublishedAt).FirstOrDefault();

        if (latest == null)
        {
            return new Result<GDManVersionInfo>
            {
                Error = ["Error checking for updates"],
            };
        }

        return new()
        {
            Status = ResultStatus.OK,
            Value = new(SemanticVersioning.Version.Parse(latest.TagName), latest.HtmlUrl)
        };
    }
}