using CalqFramework.Cmd.AspNetCore.Durability;
using CalqFramework.Cmd.Shell.Durable;
using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.AspNetCore;

/// <summary>
///     Action filter that automatically configures LocalTerminal for ASP.NET Core requests.
///     Sets LocalTerminal.Out to Response.Body and applies default Shell and TerminalLogger.
///     Creates per-request DurabilityContext with distributed store — enables automatic durability
///     for all endpoints. Parses caller-provided workflow headers or derives a request-scoped ID.
/// </summary>
public class LocalTerminalFilter(IOptions<CalqCmdControllerOptions> options, IDistributedCache distributedCache) : IActionFilter {
    private readonly CalqCmdControllerOptions _options = options.Value;

    public void OnActionExecuting(ActionExecutingContext context) {
        LocalTerminal.Out = context.HttpContext.Response.Body;
        LocalTerminal.TerminalLogger = _options.DefaultTerminalLogger;

        // Determine workflow ID and sequence path
        string workflowId;
        string sequencePath;

        if (context.HttpContext.Request.Headers.TryGetValue("calq_cmd_workflow_id", out StringValues wfId) && context.HttpContext.Request.Headers.TryGetValue("calq_cmd_sequence_path", out StringValues seqPath)) {
            // Caller-provided identity (from upstream DurableShell)
            workflowId = wfId!;
            sequencePath = seqPath!;
        } else {
            // Derive from request properties — stable across retries of the same request
            workflowId = ComputeRequestWorkflowId(context.HttpContext.Request);
            sequencePath = "";
        }

        // Create per-request distributed store
        DistributedCacheDurabilityStore store = new(distributedCache, $"CalqCmd:Durable:{workflowId}:{sequencePath}:");

        // Set durability context — the Shell setter will use this store
        DurabilityContext.Current.Value = new DurableWorkflowContext(workflowId, sequencePath, store);

        // Set default shell (triggers auto-wrap with the distributed store)
        LocalTerminal.Shell = _options.DefaultShell;

        // Register store for disposal at end of request
        context.HttpContext.Response.RegisterForDispose(store);
    }

    public void OnActionExecuted(ActionExecutedContext context) {
        // Dispose DurableShell if it owns private state
        if (LocalTerminal.Shell is DurableShell durableShell) {
            context.HttpContext.Response.RegisterForDispose(durableShell);
        }
    }

    private static string ComputeRequestWorkflowId(HttpRequest request) {
        string input = request.Method + "\0" + request.Path + "\0" + request.QueryString;
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash)[..16]
            .ToLowerInvariant();
    }
}
