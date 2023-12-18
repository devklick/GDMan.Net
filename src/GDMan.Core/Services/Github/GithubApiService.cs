using GDMan.Core.Models.Github;

namespace GDMan.Core.Services.Github;

public class GithubApiService(WebApiService webApiService)
{
    private readonly WebApiService _webApiService = webApiService;

    public GithubApiService() : this(new(new HttpClient
    {
        BaseAddress = new("https://api.github.com")
    }))
    { }

    public async Task<IEnumerable<Release>> GetReleasesAsync(string owner, string repo)
    {
        var result = await _webApiService.GetAsync<IEnumerable<Release>>("repos", $"{owner}/{repo}", "releases");

        if (result.Status == Models.ResultStatus.OK) return result.Value!;

        throw new Exception("Request failed, handle later");
    }


}