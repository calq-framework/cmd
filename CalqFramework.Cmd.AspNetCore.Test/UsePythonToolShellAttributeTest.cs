using CalqFramework.Cmd.Python;
using CalqFramework.Cmd.Shells;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.AspNetCore.Test;

public class UsePythonToolShellAttributeTest {
    private static ActionExecutingContext CreateEmptyContext() {
        DefaultHttpContext httpContext = new();
        ActionContext actionContext = new(httpContext, new RouteData(), new ActionDescriptor());
        return new ActionExecutingContext(actionContext, [], new Dictionary<string, object?>(),
            new object());
    }

    [Fact]
    public void UsePythonToolShellAttribute_WithProvidedShell_SetsLocalTerminalShellToProvidedPythonTool() {
        // Arrange
        Mock<IPythonToolServer> mockPythonServer = new();
        mockPythonServer.Setup(x => x.Uri).Returns(new Uri("https://localhost:8000"));
        PythonTool shell = new(mockPythonServer.Object);
        UsePythonToolShellAttribute attribute = new(shell);
        ActionExecutingContext context = CreateEmptyContext();

        // Act
        attribute.OnActionExecuting(context);

        // Assert
        Assert.Equal(shell, LocalTerminal.Shell);
    }
}
