using CalqFramework.Cmd.Shells;
using Microsoft.AspNetCore.Mvc.Filters;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.AspNetCore;

/// <summary>
/// ActionFilter attribute that sets LocalTerminal.Shell to use Bash shell for the request scope.
/// Automatically handles WSL path mapping on Windows and supports Cygwin/MinGW/MSYS2 environments.
/// </summary>
public class UseBashShellAttribute : ActionFilterAttribute
{
    private readonly Bash _shell;

    public UseBashShellAttribute()
    {
        _shell = new Bash();
    }

    public UseBashShellAttribute(Bash shell)
    {
        _shell = shell;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        LocalTerminal.Shell = _shell;
    }
}