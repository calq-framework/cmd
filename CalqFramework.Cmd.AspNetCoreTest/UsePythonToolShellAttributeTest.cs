using CalqFramework.Cmd.AspNetCore;
using CalqFramework.Cmd.Python;
using CalqFramework.Cmd.Shells;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Moq;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.AspNetCoreTest;

public class UsePythonToolShellAttributeTest
{
    private static ActionExecutingContext CreateEmptyContext()
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
        return new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), new object());
    }

    [Fact]
    public void UsePythonToolShellAttribute_WithProvidedShell_SetsLocalTerminalShellToProvidedPythonTool()
    {
        // Arrange
        var mockPythonServer = new Mock<IPythonToolServer>();
        mockPythonServer.Setup(x => x.Uri).Returns(new Uri("https://localhost:8000"));
        var shell = new PythonTool(mockPythonServer.Object);
        var attribute = new UsePythonToolShellAttribute(shell);
        var context = CreateEmptyContext();

        // Act
        attribute.OnActionExecuting(context);

        // Assert
        Assert.Equal(shell, LocalTerminal.Shell);
    }
}