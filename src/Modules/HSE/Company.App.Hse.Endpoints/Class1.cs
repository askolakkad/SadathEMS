using Company.App.Hse.Application;
using Company.App.Hse.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Company.App.Hse.Endpoints;

public static class HseModule
{
    public static IServiceCollection AddHseModule(this IServiceCollection services)
    {
        services.AddHseInfrastructure();
        services.AddScoped<SubmitIncidentCommandHandler>();
        return services;
    }

    public static IEndpointRouteBuilder MapHseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/hse");

        group.MapGet("/health", () => Results.Ok(new { Module = "HSE", Status = "Healthy" }));
        group.MapPost("/incidents", async (SubmitIncidentRequest request, SubmitIncidentCommandHandler handler, CancellationToken cancellationToken) =>
        {
            var incident = await handler.HandleAsync(new SubmitIncidentCommand(request.Title, request.OccurredOnUtc), cancellationToken);
            return Results.Created($"/api/hse/incidents/{incident.Id}", new
            {
                incident.Id,
                incident.TenantId,
                incident.Title,
                incident.OccurredOnUtc
            });
        });

        return app;
    }
}

public sealed record SubmitIncidentRequest(string Title, DateTime OccurredOnUtc);
