using CalqFramework.Cmd.AspNetCore;
using CalqFramework.Cmd.Shells;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.AspNetCoreTest;

public class UseLocalHttpToolShellAttributeTest
{
    private static ActionExecutingContext CreateEmptyContext()
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
        return new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), new object());
    }

    private static ActionExecutingContext CreateContextWithServices(IServiceProvider serviceProvider)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = serviceProvider;
        var actionContext = new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
        return new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), new object());
    }

    [Fact]
    public void UseLocalHttpToolShellAttribute_WithProvidedShell_SetsLocalTerminalShellToProvidedHttpTool()
    {
        // Arrange
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:5001/");
        var shell = new HttpTool(httpClient);
        var attribute = new UseLocalHttpToolShellAttribute(shell);
        var context = CreateEmptyContext();

        // Act
        attribute.OnActionExecuting(context);

        // Assert
        Assert.Equal(shell, LocalTerminal.Shell);
    }

    [Fact]
    public void UseLocalHttpToolShellAttribute_WithoutProvidedShell_UsesLocalHttpToolFactoryFromDI()
    {
        // Arrange
        var factory = new LocalHttpToolFactory("https://localhost:5001");

        var services = new ServiceCollection();
        services.AddSingleton(factory);
        var serviceProvider = services.BuildServiceProvider();

        var attribute = new UseLocalHttpToolShellAttribute();
        var context = CreateContextWithServices(serviceProvider);

        // Act
        attribute.OnActionExecuting(context);

        // Assert
        Assert.IsType<HttpTool>(LocalTerminal.Shell);
        var httpTool = (HttpTool)LocalTerminal.Shell;
        Assert.Equal("https://localhost:5001/", httpTool.HttpClient.BaseAddress?.ToString());
    }

    [Fact]
    public void UseLocalHttpToolShellAttribute_WithoutProvidedShell_ThrowsWhenFactoryNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        var attribute = new UseLocalHttpToolShellAttribute();
        var context = CreateContextWithServices(serviceProvider);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => attribute.OnActionExecuting(context));
    }
}