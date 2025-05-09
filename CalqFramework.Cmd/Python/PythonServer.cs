using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd.Shells;
using System.Reflection;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.Python;
public class PythonServer : IPythonServer {
    public PythonServer(string scriptPath) {
        var scriptDir = Path.GetDirectoryName(Path.GetFullPath(scriptPath))!;
        var scriptFileNameWithoutExtension = Path.GetFileNameWithoutExtension(scriptPath);
        LocalTerminal.Shell = new Bash();
        var scriptDirWihinShell = LocalTerminal.Shell.MapToInternalPath(scriptDir);

        CMD(@"openssl req -x509 -newkey rsa:2048 -keyout key.pem -out cert.pem -days 365 -nodes -subj ""/CN=localhost""");

        var assembly = Assembly.GetExecutingAssembly();
        var pythonServerFile = "CalqFramework.Cmd.Python.server.py";

        using Stream stream = assembly.GetManifestResourceStream(pythonServerFile)!;
        using StreamReader reader = new StreamReader(stream);
        var pythonServerScript = reader.ReadToEnd();

        pythonServerScript = pythonServerScript.Replace("sys.path.append('./')", $"sys.path.append('{scriptDir}')");
        pythonServerScript = pythonServerScript.Replace("test_tool", scriptFileNameWithoutExtension);

        PythonServerScript = CMDV(@$"python <<EOF
{pythonServerScript}
EOF");
    }

    public Uri Uri { get; } = new Uri("https://localhost:8443");
    private ShellScript PythonServerScript { get; }

    public async Task<IShellWorker> StartAsync(CancellationToken cancellationToken = default) {
        return await PythonServerScript.StartAsync(cancellationToken);
    }
}
