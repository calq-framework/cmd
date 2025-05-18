using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd.Shells;
using System.Reflection;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.Python;

public class PythonToolServer : IPythonToolServer {
    private int? _port;
    private bool _started;

    public PythonToolServer(string toolScriptPath) {
        ToolScriptPath = toolScriptPath;
    }

    public int Port {
        get => !_started ? throw new InvalidOperationException("Server hasn't started yet.") : (int)_port!;
        init => _port = value;
    }

    public IShell Shell { get; init; } = new Bash();
    public string ToolScriptPath { get; }

    public Uri Uri {
        get => !_started ? throw new InvalidOperationException("Server hasn't started yet.") : new Uri($"https://localhost:{Port}");
    }

    public async Task<IShellWorker> StartAsync(CancellationToken cancellationToken = default) {
        if (_started) {
            throw new InvalidOperationException("Server has already started.");
        }

        var scriptDir = Path.GetDirectoryName(Path.GetFullPath(ToolScriptPath))!;
        var scriptFileNameWithoutExtension = Path.GetFileNameWithoutExtension(ToolScriptPath);

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

        _port ??= GetAvailablePort();
        pythonServerScript = pythonServerScript.Replace("8443", _port.ToString());

        var cmd = CMDV(@$"python <<EOF
{pythonServerScript}
EOF");

        var worker = await cmd.StartAsync(cancellationToken);
        _started = true;

        return worker;
    }

    private int GetAvailablePort() {
        var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}