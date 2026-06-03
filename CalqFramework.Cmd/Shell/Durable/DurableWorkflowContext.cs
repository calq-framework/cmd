namespace CalqFramework.Cmd.Shell.Durable;

/// <summary>
///     Carries workflow identity, position, and store across async boundaries for distributed durability.
///     When Store is non-null, the Shell setter uses it instead of the default filesystem store.
/// </summary>
public record DurableWorkflowContext(string WorkflowId, string SequencePath, IDurabilityStore Store);
