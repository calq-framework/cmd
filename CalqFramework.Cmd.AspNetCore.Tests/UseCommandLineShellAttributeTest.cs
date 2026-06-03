using CalqFramework.Cmd.AspNetCore.Attributes;
using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.AspNetCore.Tests;

public class UseCommandLineShellAttributeTest {
    private static ActionExecutingContext CreateEmptyContext() {
        DefaultHttpContext httpContext = new();
        ActionContext actionContext = new(httpContext, new RouteData(), new ActionDescriptor());
        return new ActionExecutingContext(actionContext, [], new Dictionary<string, object?>(), new object());
    }

    [Fact]
    public void UseCommandLineShellAttribute_SetsLocalTerminalShellToDurableShell() {
        // Arrange
        UseCommandLineShellAttribute attribute = new();
        ActionExecutingContext context = CreateEmptyContext();

        // Act
        attribute.OnActionExecuting(context);

        // Assert — auto-wrapped in DurableShell (durability by default)
        Assert.IsType<DurableShell>(LocalTerminal.Shell);
    }

    [Fact]
    public void UseCommandLineShellAttribute_WithProvidedShell_DelegatesToCommandLine() {
        // Arrange
        CommandLine shell = new();
        UseCommandLineShellAttribute attribute = new(shell);
        ActionExecutingContext context = CreateEmptyContext();

        // Act
        attribute.OnActionExecuting(context);

        // Assert — wrapped in DurableShell, transparent delegation
        Assert.IsType<DurableShell>(LocalTerminal.Shell);
        Assert.Same(shell.ExceptionFactory, LocalTerminal.Shell.ExceptionFactory);
    }
}
