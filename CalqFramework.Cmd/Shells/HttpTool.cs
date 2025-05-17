using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd.Shells;
public class HttpTool : ShellBase {
    public HttpTool(HttpClient httpClient) {
        HttpClient = httpClient;
    }

    public HttpClient HttpClient { get; }

    public override string MapToInternalPath(string hostPath) {
        return hostPath;
    }

    public override IShellWorker CreateShellWorker(ShellScript shellScript, Stream? inputStream) {
        return new HttpToolWorker(HttpClient, shellScript, inputStream);
    }
}
