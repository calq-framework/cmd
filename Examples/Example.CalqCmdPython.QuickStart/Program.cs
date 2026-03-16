using CalqFramework.Cmd.Python;
using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

// Start PythonToolServer with the Python Fire script
var pts = new PythonToolServer("tool.py");
using var worker = await pts.StartAsync();
LocalTerminal.Shell = new PythonTool(pts);

// Call Python functions like shell commands
RUN("add 9 1");           // prints "10"
RUN("upper --msg world"); // prints "WORLD"

// Get return values
string result = CMD("add 9 1");
Console.WriteLine(result); // "10"
