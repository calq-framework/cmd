using CalqFramework.Cmd.Shells;
using Microsoft.AspNetCore.Mvc.Filters;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.AspNetCore;

/// <summary>
/// ActionFilter attribute that sets LocalTerminal.Shell to use PythonTool shell for the request scope.
/// Enables high-performance Python script execution via HTTP/2 communication with PythonToolServer.
/// </summary>
public class UsePythonToolShellAttribute : ActionFilterAttribute
{
    private readonly PythonTool _shell;

    public UsePythonToolShellAttribute(PythonTool shell)
    {
        _shell = shell;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        LocalTerminal.Shell = _shell;
    }
}