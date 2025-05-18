using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd.Shells;
public class HttpTool : ShellBase {
    public HttpTool(HttpClient httpClient) {
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
