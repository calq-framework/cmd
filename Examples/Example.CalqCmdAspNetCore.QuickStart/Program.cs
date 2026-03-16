using CalqFramework.Cmd.AspNetCore;
using CalqFramework.Cmd.Shells;
using Example.CalqCmdAspNetCore.QuickStart;
using static CalqFramework.Cmd.Terminal;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers()
    .AddApplicationPart(typeof(CalqCmdController).Assembly);
builder.Services.AddCalqCmdController(new QuickStartCommands());

var app = builder.Build();
app.MapControllers();
app.Run();

namespace Example.CalqCmdAspNetCore.QuickStart {
    public class QuickStartCommands {
        public int Add(int a, int b) => a + b;

        public int AddTwice(int a, int b) {
            LocalTerminal.Shell = new LocalTool();
            int first = CMD<int>($"Add --a {a} --b {b}");
            return CMD<int>($"Add --a {first} --b {first}");
        }
    }
}
