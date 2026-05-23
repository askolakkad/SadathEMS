using BuildingBlocks.Core;
using BuildingBlocks.Workflow;
using Company.App.Hse.Domain;

namespace Company.App.Hse.Application;

public sealed record SubmitIncidentCommand(string Title, DateTime OccurredOnUtc);

public sealed class SubmitIncidentCommandHandler
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IWorkflowRuntime _workflowRuntime;

    public SubmitIncidentCommandHandler(ITenantProvider tenantProvider, IWorkflowRuntime workflowRuntime)
    {
        _tenantProvider = tenantProvider;
        _workflowRuntime = workflowRuntime;
    }

    public async Task<Incident> HandleAsync(SubmitIncidentCommand command, CancellationToken cancellationToken = default)
    {
        var incident = new Incident(_tenantProvider.GetTenantId(), command.Title, command.OccurredOnUtc);

        await _workflowRuntime.StartWorkflowAsync(
            "HseIncidentApprovalFlow",
            new Dictionary<string, object?>
            {
                ["IncidentId"] = incident.Id,
                ["TenantId"] = incident.TenantId
            },
            cancellationToken);

        return incident;
    }
}
