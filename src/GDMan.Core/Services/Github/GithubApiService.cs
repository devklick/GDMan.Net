namespace GDMan.Core.Services.Github;

public class GithubApiService
{
    private readonly WebApiService _webApiService;

    public GithubApiService()
    {
        _webApiService = new(new HttpClient
        {
            BaseAddress = new("https://api.github.com")
        });
    }

    public GithubApiService(WebApiService webApiService)
    {
        _webApiService = webApiService;
    }


}