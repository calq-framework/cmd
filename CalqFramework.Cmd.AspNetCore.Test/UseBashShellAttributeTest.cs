using CalqFramework.Cmd.Shells;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.AspNetCore.Test;

public class UseBashShellAttributeTest {
    private static ActionExecutingContext CreateEmptyContext() {
        DefaultHttpContext httpContext = new();
        ActionContext actionContext = new(httpContext, new RouteData(), new ActionDescriptor());
        return new ActionExecutingContext(actionContext, [], new Dictionary<string, object?>(),
            new object());
    }

    [Fact]
    public void UseBashShellAttribute_SetsLocalTerminalShellToBash() {
        // Arrange
        UseBashShellAttribute attribute = new();
        ActionExecutingContext context = CreateEmptyContext();

        // Act
        attribute.OnActionExecuting(context);

        // Assert
        Assert.IsType<Bash>(LocalTerminal.Shell);
    }

    [Fact]
    public void UseBashShellAttribute_WithProvidedShell_SetsLocalTerminalShellToProvidedBash() {
        // Arrange
        Bash shell = new();
        UseBashShellAttribute attribute = new(shell);
        ActionExecutingContext context = CreateEmptyContext();

        // Act
        attribute.OnActionExecuting(context);

        // Assert
        Assert.Equal(shell, LocalTerminal.Shell);
    }
}
