using Example.CalqCmdAspNetCorePython.QuickStart;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers()
    .AddApplicationPart(typeof(CalqCmdController).Assembly);
builder.Services.AddPythonTool("tool.py");
builder.Services.AddCalqCmdController(provider => new QuickStartCommands(provider.GetRequiredService<PythonTool>()));

var app = builder.Build();
await app.Services.StartPythonToolServerAsync();
app.MapControllers();
app.Run();

namespace Example.CalqCmdAspNetCorePython.QuickStart {
    public class QuickStartCommands {
        private readonly PythonTool _pythonTool;

        public QuickStartCommands(PythonTool pythonTool) => _pythonTool = pythonTool;

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
