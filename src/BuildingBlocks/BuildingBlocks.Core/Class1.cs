namespace BuildingBlocks.Core;

public interface ITenantProvider
{
    string GetTenantId();
    string GetTenantConnectionString();
}

public interface IMultiTenantEntity
{
    string TenantId { get; }
}

public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}

public abstract class TenantEntity : Entity, IMultiTenantEntity
{
    public string TenantId { get; protected set; }

    protected TenantEntity(string tenantId)
    {
        TenantId = tenantId;
    }
}
