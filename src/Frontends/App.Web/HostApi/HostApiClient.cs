using System.Net.Http.Json;

namespace SadathEMS.AppWeb.HostApi;

public sealed class HostApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string? _cookieHeader;

    public HostApiClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _cookieHeader = httpContextAccessor.HttpContext?.Request.Headers.Cookie.ToString();
    }

    public async Task<T?> GetFromJsonAsync<T>(string requestUri, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var response = await SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
    }

    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(_cookieHeader) && !request.Headers.Contains("Cookie"))
        {
            request.Headers.Add("Cookie", _cookieHeader);
        }

        var client = _httpClientFactory.CreateClient("HostApi");
        if (request.RequestUri is not null && !request.RequestUri.IsAbsoluteUri)
        {
            var httpRequest = _httpContextAccessor.HttpContext?.Request;
            if (httpRequest is not null)
            {
                request.RequestUri = new Uri($"{httpRequest.Scheme}://{httpRequest.Host}{httpRequest.PathBase}/{request.RequestUri.OriginalString.TrimStart('/')}");
            }
        }

        return client.SendAsync(request, cancellationToken);
    }
}
