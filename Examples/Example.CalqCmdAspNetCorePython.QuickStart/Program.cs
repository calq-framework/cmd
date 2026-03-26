using CalqFramework.Cmd.AspNetCore;
using CalqFramework.Cmd.Shells;
using Example.CalqCmdAspNetCorePython.QuickStart;
using static CalqFramework.Cmd.Terminal;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers()
    .AddApplicationPart(typeof(CalqCmdController).Assembly);
builder.Services.AddPythonTool("tool.py");
builder.Services.AddCalqCmdController(provider => new QuickStartCommands(provider.GetRequiredService<PythonTool>()));

WebApplication app = builder.Build();
await app.Services.StartPythonToolServerAsync();
app.MapControllers();
app.Run();

namespace Example.CalqCmdAspNetCorePython.QuickStart {
    public class QuickStartCommands(PythonTool pythonTool) {
        private readonly PythonTool _pythonTool = pythonTool;

        public string Add(int x, int y) {
            LocalTerminal.Shell = _pythonTool;
            return CMD($"add {x} {y}");
        }

        public string Upper(string msg) {
            LocalTerminal.Shell = _pythonTool;
            return CMD($"upper --msg {msg}");
        }
    }
}
