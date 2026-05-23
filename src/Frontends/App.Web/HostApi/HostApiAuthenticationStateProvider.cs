using System.Security.Claims;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace SadathEMS.AppWeb.HostApi;

public sealed class HostApiAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly HostApiClient _hostApiClient;
    private Task<AuthenticationState>? _authenticationStateTask;

    public HostApiAuthenticationStateProvider(HostApiClient hostApiClient)
    {
        _hostApiClient = hostApiClient;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return _authenticationStateTask ??= LoadAuthenticationStateAsync();
    }

    private async Task<AuthenticationState> LoadAuthenticationStateAsync()
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/identity/session");
            using var response = await _hostApiClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return Anonymous;
            }

            var session = await response.Content.ReadFromJsonAsync<IdentitySessionViewModel>();
            if (session is null || !session.IsAuthenticated)
            {
                return Anonymous;
            }

            var claims = new List<Claim>();

            if (!string.IsNullOrWhiteSpace(session.UserId))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, session.UserId));
            }

            if (!string.IsNullOrWhiteSpace(session.Name))
            {
                claims.Add(new Claim(ClaimTypes.Name, session.Name));
            }

            if (!string.IsNullOrWhiteSpace(session.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, session.Email));
            }

            foreach (var role in session.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "HostApiCookie")));
        }
        catch
        {
            return Anonymous;
        }
    }

    private sealed class IdentitySessionViewModel
    {
        public bool IsAuthenticated { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string? Name { get; set; }

        public string? Email { get; set; }

        public IReadOnlyList<string> Roles { get; set; } = [];
    }
}
