using System.Text;
using CalqFramework.Cmd.Shells;
using CalqFramework.Cmd.TerminalComponents;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.AspNetCore.Test;

public class AspNetCoreIntegrationTest {
    private async Task<IHost> CreateTestHostAsync() {
        IHostBuilder hostBuilder = new HostBuilder()
            .ConfigureWebHost(webHost => {
                webHost.UseTestServer();
                webHost.ConfigureServices(services => {
                    services.AddControllers()
                        .AddApplicationPart(typeof(AspNetCoreIntegrationTest).Assembly);
                    services.AddCalqCmdController(new object()); // Required for LocalTool.Factory setup
                });
                webHost.Configure(app => {
                    app.UseRouting();
                    app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
                });
            });

        return await hostBuilder.StartAsync();
    }

    [Fact]
    public async Task CmdEndpoint_WithCustomAttribute_ExecutesCommandSuccessfully() {
        IHost host = await CreateTestHostAsync();
        TestServer server = host.GetTestServer();
        HttpClient client = server.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/testapi/cmd");
        string result = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.NotEmpty(result);

        Assert.Matches(@"\d+\.\d+\.\d+", result);
    }

    [Fact]
    public async Task RunEndpoint_WithCustomAttribute_ExecutesCommandSuccessfully() {
        IHost host = await CreateTestHostAsync();
        TestServer server = host.GetTestServer();
        HttpClient client = server.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/testapi/run");
        string result = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.NotEmpty(result);

        Assert.Matches(@"\d+\.\d+\.\d+", result);
    }
}

public class UseMyCustomShellAttribute : ActionFilterAttribute {
    public override void OnActionExecuting(ActionExecutingContext context) {
        LocalTerminal.TerminalLogger = new NullTerminalLogger();
        LocalTerminal.Shell = new CommandLine();
        LocalTerminal.Out = context.HttpContext.Response.Body;
    }
}

[ApiController]
[Route("[controller]")]
[UseMyCustomShell]
public class TestApiController : ControllerBase {    
    [HttpGet("cmd")]
    public IActionResult TestCmd() {
        string result = CMD("dotnet --version");
        return Ok(result);
    }

    [HttpGet("run")]
    public async Task TestRun() {
        await RUNAsync("dotnet --version");
    }
}
