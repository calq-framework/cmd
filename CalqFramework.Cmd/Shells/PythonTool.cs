using CalqFramework.Cmd.Python;
using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd.Shells;
public class PythonTool : ShellBase {

    public PythonTool(IPythonToolServer pythonServer) {
        var handler = new HttpClientHandler {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        var httpClient = new HttpClient(handler);
        httpClient.BaseAddress = pythonServer.Uri;

        HttpClient = httpClient;
    }

    public HttpClient HttpClient { get; }

    public override IShellWorker CreateShellWorker(ShellScript shellScript, Stream? inputStream) {
        return new HttpToolWorker(HttpClient, shellScript, inputStream);
    }

    public override string MapToHostPath(string internalPth) {
        return internalPth;
    }

    public override string MapToInternalPath(string hostPath) {
        return hostPath;
    }
}
