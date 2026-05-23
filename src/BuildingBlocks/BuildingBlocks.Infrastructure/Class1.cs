using BuildingBlocks.Core;

namespace BuildingBlocks.Infrastructure;

public sealed class HeaderTenantProvider : ITenantProvider
{
    private readonly string _tenantId;

    public HeaderTenantProvider(string tenantId, string connectionString)
    {
        _tenantId = string.IsNullOrWhiteSpace(tenantId) ? "default" : tenantId;
        ConnectionString = connectionString;
    }

    public string ConnectionString { get; }

    public string GetTenantId() => _tenantId;

    public string GetTenantConnectionString() => ConnectionString;
}
