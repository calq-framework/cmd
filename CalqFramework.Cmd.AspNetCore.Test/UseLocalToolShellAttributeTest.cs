using CalqFramework.Cmd.AspNetCore.Attributes;
using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.AspNetCore.Test;

public class UseLocalToolShellAttributeTest {
    private static ActionExecutingContext CreateEmptyContext() {
        DefaultHttpContext httpContext = new();
        ActionContext actionContext = new(httpContext, new RouteData(), new ActionDescriptor());
        return new ActionExecutingContext(actionContext, [], new Dictionary<string, object?>(), new object());
    }

    [Fact]
    public void UseLocalToolShellAttribute_SetsLocalTerminalShellToLocalTool() {
        // Arrange
        UseLocalToolShellAttribute attribute = new();
        ActionExecutingContext context = CreateEmptyContext();

        // Act
        attribute.OnActionExecuting(context);

        // Assert
        Assert.IsType<LocalTool>(LocalTerminal.Shell);
    }
}
