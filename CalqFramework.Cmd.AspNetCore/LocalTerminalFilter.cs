using CalqFramework.Cmd.Shells;
using CalqFramework.Cmd.TerminalComponents;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.AspNetCore;

/// <summary>
///     Action filter that automatically configures LocalTerminal for ASP.NET Core requests.
///     Sets LocalTerminal.Out to Response.Body and applies default Shell and TerminalLogger.
/// </summary>
public class LocalTerminalFilter : IActionFilter {
    private readonly CalqCmdControllerOptions _options;

    public LocalTerminalFilter(IOptions<CalqCmdControllerOptions> options) {
        _options = options.Value;
    }

    public void OnActionExecuting(ActionExecutingContext context) {
        LocalTerminal.Out = context.HttpContext.Response.Body;
        LocalTerminal.Shell = _options.DefaultShell;
        LocalTerminal.TerminalLogger = _options.DefaultTerminalLogger;
    }

    public void OnActionExecuted(ActionExecutedContext context) {
        // No cleanup needed - AsyncLocal will be cleared when context ends
    }
}
