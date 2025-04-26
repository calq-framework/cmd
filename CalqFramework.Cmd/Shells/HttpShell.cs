using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd.Shells;
public class HttpShell : ShellBase {
    public HttpShell(HttpClient httpClient) {
        HttpClient = httpClient;
    }

    public HttpClient HttpClient { get; }

    public override string MapToInternalPath(string hostPath) {
        return hostPath;
    }

    public override IShellWorker CreateShellWorker(ShellScript shellScript, Stream? inputStream) {
        return new HttpShellWorker(HttpClient, shellScript, inputStream);
    }
}
