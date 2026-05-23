using BuildingBlocks.Core;

namespace Company.App.Hse.Domain;

public sealed class Incident : TenantEntity
{
    public Incident(string tenantId, string title, DateTime occurredOnUtc)
        : base(tenantId)
    {
        Title = title;
        OccurredOnUtc = occurredOnUtc;
    }

    public string Title { get; private set; }

    public DateTime OccurredOnUtc { get; private set; }
}
