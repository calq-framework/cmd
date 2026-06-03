using CalqFramework.Cmd.Shell.Durable;

namespace CalqFramework.Cmd.AspNetCore.Durability;

/// <summary>
///     Client side: propagates durability context on outbound HTTP calls.
///     Reads AsyncLocal synchronously before any await — safe for parallel execution.
///     No-op when DurabilityContext.Current.Value is null.
/// </summary>
public class DurabilityPropagationHandler : DelegatingHandler {
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) {
        if (DurabilityContext.Current.Value is { } ctx) {
            request.Headers.Add("calq_cmd_workflow_id", ctx.WorkflowId);
            request.Headers.Add("calq_cmd_sequence_path", ctx.SequencePath);
        }

        return await base.SendAsync(request, ct);
    }
}
