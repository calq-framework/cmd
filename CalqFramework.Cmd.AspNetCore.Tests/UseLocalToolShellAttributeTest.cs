using CalqFramework.Cmd.AspNetCore.Attributes;
using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.AspNetCore.Tests;

public class UseLocalToolShellAttributeTest {
    private static ActionExecutingContext CreateEmptyContext() {
        DefaultHttpContext httpContext = new();
        ActionContext actionContext = new(httpContext, new RouteData(), new ActionDescriptor());
        return new ActionExecutingContext(actionContext, [], new Dictionary<string, object?>(), new object());
    }

    [Fact]
    public void UseLocalToolShellAttribute_SetsLocalTerminalShellToDurableShell() {
        // Arrange
        UseLocalToolShellAttribute attribute = new();
        ActionExecutingContext context = CreateEmptyContext();

        // Act
        attribute.OnActionExecuting(context);

        // Assert — auto-wrapped in DurableShell (durability by default)
        Assert.IsType<DurableShell>(LocalTerminal.Shell);
    }
}
