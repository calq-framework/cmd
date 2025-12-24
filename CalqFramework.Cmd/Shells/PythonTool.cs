using CalqFramework.Cmd.Python;
using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd.Shells;

/// <summary>
/// Python shell implementation using HTTP/2 communication with PythonToolServer.
/// Supports Python Fire compatibility and real-time streaming via async generators.
/// Provides sub-millisecond latency for Python script execution.
/// </summary>

public class PythonTool : ShellBase {

    public PythonTool(IPythonToolServer pythonServer) {
        var handler = new HttpClientHandler {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        var httpClient = new HttpClient(handler) {
            BaseAddress = pythonServer.Uri
        };

        HttpClient = httpClient;
    }

    public HttpClient HttpClient { get; }

    public override IShellWorker CreateShellWorker(ShellScript shellScript, Stream? inputStream, bool disposeOnCompletion = true) {
        return new HttpToolWorker(HttpClient, shellScript, inputStream, disposeOnCompletion);
    }

    public override string MapToHostPath(string internalPath) {
        return Path.GetFullPath(internalPath); ;
    }

    public override string MapToInternalPath(string hostPath) {
        return Path.GetFullPath(hostPath); ;
    }
}
