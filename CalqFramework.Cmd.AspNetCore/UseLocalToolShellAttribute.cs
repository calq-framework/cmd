using CalqFramework.Cmd.Shells;
using Microsoft.AspNetCore.Mvc.Filters;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.AspNetCore;

/// <summary>
/// Action filter attribute that configures LocalTool shell for the request.
/// LocalTool automatically adapts between local process execution and distributed HTTP execution
/// based on the runtime context, enabling seamless local-to-distributed scaling.
/// </summary>
public class UseLocalToolShellAttribute : ActionFilterAttribute
{
    /// <summary>
    /// Sets LocalTerminal.Shell to LocalTool before action execution.
    /// LocalTool uses LocalToolFactory to determine the appropriate underlying shell:
    /// - In development: uses local process execution via CommandLine shell
    /// - In production with CalqCmdController: uses HTTP-based distributed execution
    /// </summary>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        LocalTerminal.Shell = new LocalTool();
    }
}