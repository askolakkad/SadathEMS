using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Blazor;
using Syncfusion.Licensing;

namespace App.SharedUI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedUi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSyncfusionBlazor();

        var licenseKey = configuration["Syncfusion:LicenseKey"];
        if (!string.IsNullOrWhiteSpace(licenseKey))
        {
            SyncfusionLicenseProvider.RegisterLicense(licenseKey);
        }

        return services;
    }
}
