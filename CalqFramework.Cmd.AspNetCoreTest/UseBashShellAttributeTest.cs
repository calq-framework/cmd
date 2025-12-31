using CalqFramework.Cmd.AspNetCore;
using CalqFramework.Cmd.Shells;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.AspNetCoreTest;

public class UseBashShellAttributeTest
{
    private static ActionExecutingContext CreateEmptyContext()
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
        return new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), new object());
    }

    [Fact]
    public void UseBashShellAttribute_SetsLocalTerminalShellToBash()
    {
        // Arrange
        var attribute = new UseBashShellAttribute();
        var context = CreateEmptyContext();

        // Act
        attribute.OnActionExecuting(context);

        // Assert
        Assert.IsType<Bash>(LocalTerminal.Shell);
    }

    [Fact]
    public void UseBashShellAttribute_WithProvidedShell_SetsLocalTerminalShellToProvidedBash()
    {
        // Arrange
        var shell = new Bash();
        var attribute = new UseBashShellAttribute(shell);
        var context = CreateEmptyContext();

        // Act
        attribute.OnActionExecuting(context);

        // Assert
        Assert.Equal(shell, LocalTerminal.Shell);
    }
}