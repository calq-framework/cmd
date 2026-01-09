using CalqFramework.Cmd.Shells;
using Microsoft.AspNetCore.Mvc.Filters;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.AspNetCore;

/// <summary>
/// ActionFilter attribute that sets LocalTerminal.Shell to use LocalTool shell for the request scope.
/// LocalTool executes the current executable as a shell command, enabling self-hosting scenarios.
/// </summary>
public class UseLocalToolShellAttribute : ActionFilterAttribute
{
    private readonly LocalTool _shell;

    public UseLocalToolShellAttribute()
    {
        _shell = new LocalTool();
    }

    public UseLocalToolShellAttribute(LocalTool shell)
    {
        _shell = shell;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        LocalTerminal.Shell = _shell;
    }
}