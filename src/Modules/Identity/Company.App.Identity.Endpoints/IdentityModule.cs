using Company.App.Identity.Infrastructure;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Company.App.Identity.Application;

namespace Company.App.Identity.Endpoints;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityInfrastructure(configuration);
        return services;
    }

    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/identity");
        group.MapGet("/health", () => Results.Ok(new { Module = "Identity", Status = "Healthy" }));
        group.MapGet("/session", (HttpContext httpContext) => Results.Ok(new IdentitySessionResponse(
            httpContext.User.Identity?.IsAuthenticated == true,
            httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
            httpContext.User.Identity?.Name,
            httpContext.User.FindFirstValue(ClaimTypes.Email),
            httpContext.User.FindAll(ClaimTypes.Role).Select(role => role.Value).Distinct().ToArray())));
        group.MapGet("/administration", async Task<IResult> (
            HttpContext httpContext,
            IIdentityAdministrationService administrationService,
            CancellationToken cancellationToken) =>
        {
            if (httpContext.User.Identity?.IsAuthenticated != true || !httpContext.User.IsInRole("Administrator"))
            {
                return Results.Forbid();
            }

            var snapshot = await administrationService.GetSnapshotAsync(cancellationToken);
            return Results.Ok(snapshot);
        });

        MapIdentityWebRoutes(app);
        return app;
    }

    private static void MapIdentityWebRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/account");

        group.MapPost("/login", async Task<IResult> (HttpContext httpContext, HttpRequest request, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager) =>
        {
            var form = await request.ReadFormAsync();
            var email = form["email"].ToString().Trim();
            var password = form["password"].ToString();
            var rememberMe = form["rememberMe"].Contains("true");
            var returnUrl = NormalizeReturnUrl(httpContext, form["returnUrl"].ToString());
            var loginUrl = NormalizeReturnUrl(httpContext, form["loginUrl"].ToString(), "/login");

            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                return Results.Redirect(BuildLoginFailureUrl(loginUrl, "invalid", returnUrl, email, rememberMe));
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                await signInManager.SignInAsync(user, isPersistent: rememberMe);
                return Results.Redirect(returnUrl);
            }

            var error = result.IsLockedOut ? "lockedout" : "invalid";
            return Results.Redirect(BuildLoginFailureUrl(loginUrl, error, returnUrl, email, rememberMe));
        });

        group.MapGet("/logout", async Task<IResult> (HttpContext httpContext, SignInManager<ApplicationUser> signInManager, string? returnUrl) =>
        {
            await signInManager.SignOutAsync();
            return Results.Redirect(NormalizeReturnUrl(httpContext, returnUrl, "/login"));
        });
    }

    private static string BuildLoginFailureUrl(string loginUrl, string error, string returnUrl, string email, bool rememberMe)
    {
        return QueryHelpers.AddQueryString(loginUrl, new Dictionary<string, string?>
        {
            ["error"] = error,
            ["returnUrl"] = returnUrl,
            ["email"] = email,
            ["rememberMe"] = rememberMe ? "true" : "false"
        });
    }

    private static string NormalizeReturnUrl(HttpContext httpContext, string? returnUrl, string defaultUrl = "/")
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return defaultUrl;
        }

        if (returnUrl.StartsWith('/') && !returnUrl.StartsWith("//"))
        {
            return returnUrl;
        }

        if (Uri.TryCreate(returnUrl, UriKind.Absolute, out var absoluteUri)
            && (absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps)
            && string.Equals(absoluteUri.Host, httpContext.Request.Host.Host, StringComparison.OrdinalIgnoreCase))
        {
            return absoluteUri.ToString();
        }

        return defaultUrl;
    }

    private sealed record IdentitySessionResponse(
        bool IsAuthenticated,
        string UserId,
        string? Name,
        string? Email,
        IReadOnlyList<string> Roles);
}
