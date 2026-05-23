namespace BuildingBlocks.Workflow;

public interface IWorkflowRuntime
{
    Task StartWorkflowAsync(string workflowName, IReadOnlyDictionary<string, object?> input, CancellationToken cancellationToken = default);
}

public sealed class NoOpWorkflowRuntime : IWorkflowRuntime
{
    public Task StartWorkflowAsync(string workflowName, IReadOnlyDictionary<string, object?> input, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
