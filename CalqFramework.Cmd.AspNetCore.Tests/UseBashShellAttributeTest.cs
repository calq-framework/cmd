using CalqFramework.Cmd.AspNetCore.Attributes;
using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.AspNetCore.Tests;

public class UseBashShellAttributeTest {
    private static ActionExecutingContext CreateEmptyContext() {
        DefaultHttpContext httpContext = new();
        ActionContext actionContext = new(httpContext, new RouteData(), new ActionDescriptor());
        return new ActionExecutingContext(actionContext, [], new Dictionary<string, object?>(), new object());
    }

    [Fact]
    public void UseBashShellAttribute_SetsLocalTerminalShellToDurableShell() {
        // Arrange
        UseBashShellAttribute attribute = new();
        ActionExecutingContext context = CreateEmptyContext();

        // Act
        attribute.OnActionExecuting(context);

        // Assert — auto-wrapped in DurableShell (durability by default)
        Assert.IsType<DurableShell>(LocalTerminal.Shell);
    }

    [Fact]
    public void UseBashShellAttribute_WithProvidedShell_DelegatesToBash() {
        // Arrange
        Bash shell = new();
        UseBashShellAttribute attribute = new(shell);
        ActionExecutingContext context = CreateEmptyContext();

        // Act
        attribute.OnActionExecuting(context);

        // Assert — wrapped in DurableShell, transparent delegation to provided Bash
        Assert.IsType<DurableShell>(LocalTerminal.Shell);
        Assert.Same(shell.ExceptionFactory, LocalTerminal.Shell.ExceptionFactory);
        Assert.Same(shell.Postprocessor, LocalTerminal.Shell.Postprocessor);
    }
}
