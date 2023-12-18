using System.Net;
using System.Text;
using System.Text.Json;

using GDMan.Core.Extensions;
using GDMan.Core.Models;

namespace GDMan.Core.Services;

public class WebApiService(HttpClient client)
{
    private readonly HttpClient _client = client;


    /// <summary>
    /// Performs an HTTP GET request on the specified endpoint, capturing and deserializing the 
    /// response content (if there is any) to the specified type.
    /// </summary>
    /// <typeparam name="T">The type for the response content to be deserialized to</typeparam>
    /// <param name="baseUrl">The base url for the endpoint. This would generally be the host e.g. http://mywebsite.com</param>
    /// <param name="controller">The controller where the resource can be found. Generally this follows on after the baseUrl followed by a slash</param>
    /// <param name="action">The name of the action on the controller. Generally this follows on after the controller followed by a slash</param>
    /// <param name="parameters">The URL query parameter string of URL encoded keys and values</param>
    /// <param name="trimTrailingSlash">Indicates whether or not any trailing forward slashes should be trimmed from the URL before sending the request</param>
    /// <returns><see cref="GenericResult{T}"/> representing the result of the HTTP call, where T is the deserialized response content/></returns>
    public async Task<Result<T>> GetAsync<T>(string baseUrl, string? controller = null, string? action = null, string? queryParameters = null, bool trimTrailingSlash = true)
    {
        var result = new Result<T>();
        try
        {
            var endpoint = BuildEndpointUrl(baseUrl, controller, action, queryParameters, trimTrailingSlash);
            var httpResponse = await _client.GetAsync(endpoint);
            result = await ProcessResponse<T>(httpResponse);
        }
        catch (Exception ex)
        {
            result.Messages = [ex.Message];
        }
        return result;
    }

    /// <summary>
    /// Constructs a <see cref="Uri"/> object based on the specified values.
    /// </summary>
    /// <param name="baseUrl">The base url for the endpoint. This would generally be the host e.g. http://mywebsite.com</param>
    /// <param name="controller">The controller where the resource can be found. Generally this follows on after the baseUrl followed by a slash</param>
    /// <param name="action">The name of the action on the controller. Generally this follows on after the controller followed by a slash</param>
    /// <param name="parameters">The URL query parameter string of URL encoded keys and values</param>
    /// <param name="trimTrailingSlash">Indicates whether or not any trailing forward slashes should be trimmed from the URL before sending the request</param>
    /// <returns><see cref="Uri"/>object for with the given values</returns>
    private static string BuildEndpointUrl(string baseUrl, string? controller, string? action, string? parameters = null, bool trimTrailingSlash = true)
    {
        string url = baseUrl.EnsureTrailingChar('/');

        url += string.IsNullOrEmpty(controller) ? string.Empty : controller.EnsureTrailingChar('/');
        url += string.IsNullOrEmpty(action) ? string.Empty : action.EnsureTrailingChar('/');
        url += string.IsNullOrEmpty(parameters) ? string.Empty : parameters.EnsureTrailingChar('/');

        if (trimTrailingSlash)
        {
            url = url.TrimEnd('/');
        }

        return url;
    }

    /// <summary>
    /// Sends a HTTP request using the specified HTTP method and object type. Intended to be used for POST/PUT/PATCH requests
    /// </summary>
    /// <typeparam name="TInput">The type of object being included in the request</typeparam>
    /// <typeparam name="TOutput">The type of object expected in the result</typeparam>
    /// <param name="httpMethod">The HTTP Method to be used when sending the request</param>
    /// <param name="uri">The URI location of the endpoint</param>
    /// <param name="model">The object to be included in the request</param>
    /// <param name="contentType">The content type expected by the server</param>
    /// <returns><see cref="GenericResult{TOutput}"/> representation of the HTTP response</returns>
    private async Task<Result<TOutput>> SendAsync<TInput, TOutput>(HttpMethod httpMethod, Uri uri, TInput model, ContentType contentType)
    {
        var response = new Result<TOutput>();

        try
        {
            var request = new HttpRequestMessage(httpMethod, uri);
            var contentTypeStr = contentType.GetDescription() ?? "application/json";

            if (typeof(TInput) == typeof(string))
            {
                request.Content = new StringContent(model?.ToString() ?? "", Encoding.UTF8, contentTypeStr);
            }
            else if (contentType == ContentType.Json)
            {
                request.Content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, contentTypeStr);
            }
            else
            {
                throw new NotImplementedException($"Content type ${contentType} not supported");
            }

            var httpResponse = await _client.SendAsync(request);
            response = await ProcessResponse<TOutput>(httpResponse);
        }
        catch (Exception ex)
        {
            response.Status = ResultStatus.ServerError;
            response.Messages = [ex.Message];
        }

        return response;
    }

    /// <summary>
    /// Processes the <see cref="HttpResponseMessage"/> returned by the client and creates the <see cref="GenericResult"/> 
    /// object expected by the caller.
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize the response content to</typeparam>
    /// <param name="httpResponseMessage">The http response to process</param>
    /// <returns><see cref="GenericResult{T}"/> representation of specified the <see cref="HttpResponseMessage"/></returns>
    private static async Task<Result<T>> ProcessResponse<T>(HttpResponseMessage httpResponseMessage)
    {
        // get the response content and the content type, if they exist
        var responseString = string.Empty;
        var contentType = string.Empty;

        if (httpResponseMessage.Content != null)
        {
            responseString = await httpResponseMessage.Content.ReadAsStringAsync();

            if (httpResponseMessage.Content.Headers != null)
            {
                if (httpResponseMessage.Content.Headers.TryGetValues("Content-Type", out var contentTypeList))
                {
                    contentType = contentTypeList.First();
                }
            }
        }

        // If the response is OK, deserialize the content and return the result
        if (httpResponseMessage.IsSuccessStatusCode)
        {
            return ProcessSuccessResponse<T>(contentType, responseString);
        }

        return ProcessFailureResponse<T>(contentType, responseString, httpResponseMessage.StatusCode);
    }

    /// <summary>
    /// Constructs a <see cref="GenericResult{T}"/> object with <see cref="GenericResult{T}.Status"/> set to <see cref="ResultStatus.Ok"/>.
    /// T represents the object to deserialize the <paramref name="contentString"/> to, if it has a value.
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize the response content to</typeparam>
    /// <param name="contentType">The content type indicated by the response headers, used to determine the deserialization method</param>
    /// <param name="contentString">The string representation of the response content</param>
    /// <returns><see cref="GenericResult{T}"/> with status Ok and Value an object of the deserialized <paramref name="contentString"/></returns>
    private static Result<T> ProcessSuccessResponse<T>(string contentType, string contentString)
    {
        var result = new Result<T>
        {
            Status = ResultStatus.OK
        };

        if (string.IsNullOrWhiteSpace(contentString))
        {
            return result;
        }

        if (contentType.Contains("json"))
        {
            result.Value = JsonSerializer.Deserialize<T>(contentString);
        }
        else
        {
            throw new NotImplementedException($"Unsupported content type in response: ${contentType}");
        }

        return result;
    }

    /// <summary>
    /// Constructs a <see cref="GenericResult{T}"/> object with <see cref="GenericResult{T}.Status"/> set to <see cref="ResultStatus.ExternalApiError"/>.
    /// T represents the object to deserialize the <paramref name="contentString"/> to, if it has a value.
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize the response content to</typeparam>
    /// <param name="contentType">The content type indicated by the response headers, used to determine the deserialization method</param>
    /// <param name="contentString">The string representation of the response content</param>
    /// <returns><see cref="GenericResult{T}"/> with status ExternalApiError and Messages object of the deserialized <paramref name="contentString"/> if possible,
    /// otherwise Messages will contain the a string with the response code</returns>
    private static Result<T> ProcessFailureResponse<T>(string contentType, string contentString, HttpStatusCode statusCode)
    {
        var result = new Result<T>
        {
            Status = statusCode.IsClientError() ? ResultStatus.ClientError : ResultStatus.ServerError
        };

        // If the result was not OK and no content was provided, 
        // add the status code to the result object and return the result

        if (string.IsNullOrEmpty(contentString))
        {
            result.Messages = [$"Server returned not-OK status code {statusCode} and no content."];
            return result;
        }

        // If content was provided in the response, try and deserialize it to a collection of strings.
        // If this doesn't work, simply add the response string to the result object's messages
        try
        {
            if (contentType.Contains("json"))
            {
                result.Messages = JsonSerializer.Deserialize<List<string>>(contentString) ?? [];
            }
            else
            {
                throw new NotImplementedException($"Unsupported content type in response: ${contentType}");
            }
        }
        catch
        {
            result.Messages = [
                $"HTTP Response code {statusCode}" +
                $"{Environment.NewLine}Content: {contentString}"
            ];
        }

        return result;
    }
}