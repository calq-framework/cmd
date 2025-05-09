using CalqFramework.Cmd.Python;
using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd.Shells;
public class Python : ShellBase {

    public Python(IPythonServer pythonServer) {
        var handler = new HttpClientHandler {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        var httpClient = new HttpClient(handler);
        httpClient.BaseAddress = pythonServer.Uri;

        HttpClient = httpClient;
    }

    public HttpClient HttpClient { get; }

    public override IShellWorker CreateShellWorker(ShellScript shellScript, Stream? inputStream) {
        return new HttpShellWorker(HttpClient, shellScript, inputStream);
    }

    public override string MapToInternalPath(string hostPath) {
        return hostPath;
    }
}
