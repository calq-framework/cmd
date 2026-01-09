using CalqFramework.Cmd.AspNetCore;
using CalqFramework.Cmd.Shells;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.AspNetCoreTest;

public class UseLocalToolShellAttributeTest
{
    private static ActionExecutingContext CreateEmptyContext()
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
        return new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), new object());
    }

    [Fact]
    public void UseLocalToolShellAttribute_SetsLocalTerminalShellToLocalTool()
    {
        // Arrange
        var attribute = new UseLocalToolShellAttribute();
        var context = CreateEmptyContext();

        // Act
        attribute.OnActionExecuting(context);

        // Assert
        Assert.IsType<LocalTool>(LocalTerminal.Shell);
    }

    [Fact]
    public void UseLocalToolShellAttribute_WithProvidedShell_SetsLocalTerminalShellToProvidedLocalTool()
    {
        // Arrange
        var shell = new LocalTool();
        var attribute = new UseLocalToolShellAttribute(shell);
        var context = CreateEmptyContext();

        // Act
        attribute.OnActionExecuting(context);

        // Assert
        Assert.Equal(shell, LocalTerminal.Shell);
    }
}