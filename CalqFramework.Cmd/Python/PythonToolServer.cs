using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd.Shells;
using System.Reflection;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.Python;
public class PythonToolServer : IPythonToolServer {
    public PythonToolServer(string scriptPath) {
        ScriptPath = scriptPath;
    }

    public string ScriptPath { get; }
    public Uri Uri { get; } = new Uri("https://localhost:8443");

    public IShell Shell { get; init; } = new Bash();

    public async Task<IShellWorker> StartAsync(CancellationToken cancellationToken = default) {
        var scriptDir = Path.GetDirectoryName(Path.GetFullPath(ScriptPath))!;
        var scriptFileNameWithoutExtension = Path.GetFileNameWithoutExtension(ScriptPath);

        LocalTerminal.Shell = Shell;
        var scriptDirWihinShell = LocalTerminal.Shell.MapToInternalPath(scriptDir);

        await CMDAsync(@"openssl req -x509 -newkey rsa:2048 -keyout key.pem -out cert.pem -days 365 -nodes -subj ""/CN=localhost""");

        var assembly = Assembly.GetExecutingAssembly();
        var pythonServerFile = "CalqFramework.Cmd.Python.server.py";

        using Stream stream = assembly.GetManifestResourceStream(pythonServerFile)!;
        using StreamReader reader = new StreamReader(stream);
        var pythonServerScript = reader.ReadToEnd();

        pythonServerScript = pythonServerScript.Replace("sys.path.append('./')", $"sys.path.append('{scriptDir}')");
        pythonServerScript = pythonServerScript.Replace("test_tool", scriptFileNameWithoutExtension);

        var cmd = CMDV(@$"python <<EOF
{pythonServerScript}
EOF");

        return await cmd.StartAsync(cancellationToken);
    }
}
