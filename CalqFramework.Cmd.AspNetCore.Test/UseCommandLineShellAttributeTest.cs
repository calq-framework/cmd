using CalqFramework.Cmd.AspNetCore.Attributes;
using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.AspNetCore.Test;

public class UseCommandLineShellAttributeTest {
    private static ActionExecutingContext CreateEmptyContext() {
        DefaultHttpContext httpContext = new();
        ActionContext actionContext = new(httpContext, new RouteData(), new ActionDescriptor());
        return new ActionExecutingContext(actionContext, [], new Dictionary<string, object?>(), new object());
    }

    [Fact]
    public void UseCommandLineShellAttribute_SetsLocalTerminalShellToCommandLine() {
        // Arrange
        UseCommandLineShellAttribute attribute = new();
        ActionExecutingContext context = CreateEmptyContext();

        // Act
        attribute.OnActionExecuting(context);

        // Assert
        Assert.IsType<CommandLine>(LocalTerminal.Shell);
    }

    [Fact]
    public void UseCommandLineShellAttribute_WithProvidedShell_SetsLocalTerminalShellToProvidedCommandLine() {
        // Arrange
        CommandLine shell = new();
        UseCommandLineShellAttribute attribute = new(shell);
        ActionExecutingContext context = CreateEmptyContext();

        // Act
        attribute.OnActionExecuting(context);

        // Assert
        Assert.Equal(shell, LocalTerminal.Shell);
    }
}
