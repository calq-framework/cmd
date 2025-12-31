using CalqFramework.Cmd.AspNetCore;
using CalqFramework.Cmd.Shells;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.AspNetCoreTest;

public class UseCommandLineShellAttributeTest
{
    private static ActionExecutingContext CreateEmptyContext()
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
        return new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), new object());
    }

    [Fact]
    public void UseCommandLineShellAttribute_SetsLocalTerminalShellToCommandLine()
    {
        // Arrange
        var attribute = new UseCommandLineShellAttribute();
        var context = CreateEmptyContext();

        // Act
        attribute.OnActionExecuting(context);

        // Assert
        Assert.IsType<CommandLine>(LocalTerminal.Shell);
    }

    [Fact]
    public void UseCommandLineShellAttribute_WithProvidedShell_SetsLocalTerminalShellToProvidedCommandLine()
    {
        // Arrange
        var shell = new CommandLine();
        var attribute = new UseCommandLineShellAttribute(shell);
        var context = CreateEmptyContext();

        // Act
        attribute.OnActionExecuting(context);

        // Assert
        Assert.Equal(shell, LocalTerminal.Shell);
    }
}