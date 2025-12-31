using CalqFramework.Cmd.Shells;
using Microsoft.AspNetCore.Mvc.Filters;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.AspNetCore;

/// <summary>
/// ActionFilter attribute that sets LocalTerminal.Shell to use CommandLine shell for the request scope.
/// Provides basic process execution for Windows cmd.exe or Unix shell without special path mapping.
/// </summary>
public class UseCommandLineShellAttribute : ActionFilterAttribute
{
    private readonly CommandLine _shell;

    public UseCommandLineShellAttribute()
    {
        _shell = new CommandLine();
    }

    public UseCommandLineShellAttribute(CommandLine shell)
    {
        _shell = shell;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        LocalTerminal.Shell = _shell;
    }
}