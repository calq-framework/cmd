using CalqFramework.Cmd.Python;
using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd.Shell.Http;

namespace CalqFramework.Cmd.Shells;

/// <summary>
///     Python shell implementation using HTTP/2 communication with PythonToolServer.
///     Supports Python Fire compatibility and real-time streaming via async generators.
///     Provides sub-millisecond latency for Python script execution.
/// </summary>
public class PythonTool : ShellBase {
    /// <summary>
    ///     Initializes a new PythonTool shell with SSL certificate validation disabled for local development.
    ///     Creates an HTTP client configured to communicate with the Python tool server.
    /// </summary>
    public PythonTool(IPythonToolServer pythonServer) {
        HttpClientHandler handler = new() {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        HttpClient httpClient = new(handler) {
            BaseAddress = pythonServer.Uri
        };

        HttpClient = httpClient;
    }

    /// <summary>
    ///     HTTP client configured to communicate with the Python tool server over HTTPS.
    /// </summary>
    public HttpClient HttpClient { get; }

    public override IShellWorker CreateShellWorker(ShellScript shellScript, Stream? inputStream, bool disposeOnCompletion = true) =>
        new HttpToolWorker(HttpClient, shellScript, inputStream, disposeOnCompletion);

    public override string MapToHostPath(string internalPath) => Path.GetFullPath(internalPath);

    public override string MapToInternalPath(string hostPath) => Path.GetFullPath(hostPath);
}
