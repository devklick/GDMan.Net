using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

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
    public async Task<ApiResult<T, TError>> GetAsync<T, TError>(JsonTypeInfo<T> successTypeInfo, JsonTypeInfo<TError> errorTypeInfo, string baseUrl, string? controller = null, string? action = null, string? queryParameters = null, bool trimTrailingSlash = true)
    {
        var result = new ApiResult<T, TError>();
        try
        {
            var endpoint = BuildEndpointUrl(baseUrl, controller, action, queryParameters, trimTrailingSlash);
            var httpResponse = await _client.GetAsync(endpoint);
            result = await ProcessResponse<T, TError>(httpResponse, successTypeInfo, errorTypeInfo);
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
    /// Processes the <see cref="HttpResponseMessage"/> returned by the client and creates the <see cref="GenericResult"/> 
    /// object expected by the caller.
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize the response content to</typeparam>
    /// <param name="httpResponseMessage">The http response to process</param>
    /// <returns><see cref="GenericResult{T}"/> representation of specified the <see cref="HttpResponseMessage"/></returns>
    private static async Task<ApiResult<T, TError>> ProcessResponse<T, TError>(HttpResponseMessage httpResponseMessage, JsonTypeInfo<T> successTypeInfo, JsonTypeInfo<TError> errorTypeInfo)
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
            return ProcessSuccessResponse<T, TError>(contentType, responseString, successTypeInfo);
        }

        return ProcessFailureResponse<T, TError>(contentType, responseString, httpResponseMessage.StatusCode, errorTypeInfo);
    }

    /// <summary>
    /// Constructs a <see cref="GenericResult{T}"/> object with <see cref="GenericResult{T}.Status"/> set to <see cref="ResultStatus.Ok"/>.
    /// T represents the object to deserialize the <paramref name="contentString"/> to, if it has a value.
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize the response content to</typeparam>
    /// <param name="contentType">The content type indicated by the response headers, used to determine the deserialization method</param>
    /// <param name="contentString">The string representation of the response content</param>
    /// <returns><see cref="GenericResult{T}"/> with status Ok and Value an object of the deserialized <paramref name="contentString"/></returns>
    private static ApiResult<T, TError> ProcessSuccessResponse<T, TError>(string contentType, string contentString, JsonTypeInfo<T> typeInfo)
    {
        var result = new ApiResult<T, TError>
        {
            Status = ResultStatus.OK
        };

        if (string.IsNullOrWhiteSpace(contentString))
        {
            return result;
        }

        if (contentType.Contains("json"))
        {
            result.Value = JsonSerializer.Deserialize(contentString, typeInfo);
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
    private static ApiResult<T, TError> ProcessFailureResponse<T, TError>(string contentType, string contentString, HttpStatusCode statusCode, JsonTypeInfo<TError> typeInfo)
    {
        var result = new ApiResult<T, TError>
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
                result.Error = JsonSerializer.Deserialize(contentString, typeInfo);
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