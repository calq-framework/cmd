using CalqFramework.Cmd.Shells;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.AspNetCore;

/// <summary>
/// ActionFilter attribute that sets LocalTerminal.Shell to use HttpTool shell for the request scope.
/// Automatically creates HttpTool instances using LocalHttpToolFactory to connect to local CalqCmdController.
/// </summary>
public class UseLocalHttpToolShellAttribute : ActionFilterAttribute
{
    private readonly HttpTool? _shell;

    /// <summary>
    /// Creates the attribute using LocalHttpToolFactory from DI to automatically discover CalqCmdController URL
    /// </summary>
    public UseLocalHttpToolShellAttribute()
    {
        _shell = null; // Will be resolved from DI
    }

    /// <summary>
    /// Creates the attribute with a specific HttpTool instance
    /// </summary>
    /// <param name="shell">HttpTool instance to use</param>
    public UseLocalHttpToolShellAttribute(HttpTool shell)
    {
        _shell = shell;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        HttpTool shell;
        
        if (_shell != null)
        {
            shell = _shell;
        }
        else
        {
            // Get LocalHttpToolFactory from DI and create HttpTool
            var factory = context.HttpContext.RequestServices.GetRequiredService<LocalHttpToolFactory>();
            shell = factory.CreateHttpTool();
        }
        
        LocalTerminal.Shell = shell;
    }
}