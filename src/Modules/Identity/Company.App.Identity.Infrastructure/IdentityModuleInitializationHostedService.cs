using Company.App.Identity.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Company.App.Identity.Infrastructure;

internal sealed class IdentityModuleInitializationHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<IdentityModuleInitializationHostedService> _logger;

    public IdentityModuleInitializationHostedService(
        IServiceProvider serviceProvider,
        ILogger<IdentityModuleInitializationHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var initializer = scope.ServiceProvider.GetRequiredService<IIdentityModuleInitializer>();
        await initializer.InitializeAsync(cancellationToken);
        _logger.LogInformation("Identity module startup initialization completed.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
