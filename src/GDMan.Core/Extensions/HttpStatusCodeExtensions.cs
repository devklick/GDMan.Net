using System.Net;

namespace GDMan.Core.Extensions;

public static class HttpStatusCodeExtensions
{
    public static bool IsClientError(this HttpStatusCode status)
        => status.ToString().StartsWith('4');

    public static bool IsServerError(this HttpStatusCode status)
        => status.ToString().StartsWith('5');
}