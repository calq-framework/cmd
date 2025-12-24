using System.Reflection;
using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.Python;

/// <summary>
/// HTTPS server for executing Python scripts compatible with Python Fire.
/// Embeds server.py, generates SSL certificates, and provides streaming support.
/// Input is consumed entirely before execution; output streams in real-time over HTTP/2.
/// </summary>

public class PythonToolServer(string toolScriptPath) : IPythonToolServer {
    private int? _port;
    private bool _started;

    public int Port {
        get => !_started ? throw new InvalidOperationException("Server hasn't started yet.") : (int)_port!;
        init => _port = value;
    }

    public IShell Shell { get; init; } = new CommandLine();
    public string ToolScriptPath { get; } = toolScriptPath;

    public Uri Uri {
        get => !_started ? throw new InvalidOperationException("Server hasn't started yet.") : new Uri($"https://localhost:{Port}");
    }

    public async Task<IShellWorker> StartAsync(CancellationToken cancellationToken = default) {
        if (_started) {
            throw new InvalidOperationException("Server has already started.");
        }

        string scriptDir = Path.GetDirectoryName(Path.GetFullPath(ToolScriptPath))!;
        string scriptFileNameWithoutExtension = Path.GetFileNameWithoutExtension(ToolScriptPath);

        LocalTerminal.Shell = Shell;
        string scriptDirWihinShell = LocalTerminal.Shell.MapToInternalPath(scriptDir);

        await CMDAsync(@"openssl req -x509 -newkey rsa:2048 -keyout key.pem -out cert.pem -days 365 -nodes -subj ""/CN=localhost""", cancellationToken);

        var assembly = Assembly.GetExecutingAssembly();
        string pythonServerFile = "CalqFramework.Cmd.Python.server.py";

        using Stream stream = assembly.GetManifestResourceStream(pythonServerFile)!;
        using StreamReader reader = new(stream);
        string pythonServerScript = reader.ReadToEnd();

        pythonServerScript = pythonServerScript.Replace("sys.path.append('./')", $"sys.path.append(r'{scriptDirWihinShell}')");
        pythonServerScript = pythonServerScript.Replace("test_tool", scriptFileNameWithoutExtension);

        _port ??= GetAvailablePort();
        pythonServerScript = pythonServerScript.Replace("8443", _port.ToString());

        string tempPyFile = Path.GetTempFileName();
        File.WriteAllText(tempPyFile, pythonServerScript);
        ShellScript cmd = CMDV($"python {tempPyFile}");

        IShellWorker worker = await cmd.StartAsync(cancellationToken);
        _started = true;

        return worker;
    }

    private static int GetAvailablePort() {
        var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        int port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
