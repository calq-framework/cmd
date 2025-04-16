using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd.Shells;
public class HttpShell : ShellBase {
    public HttpShell(HttpClient httpClient) {
        HttpClient = httpClient;
        ErrorHandler = new HttpShellWorkerErrorHandler();
    }

    public HttpClient HttpClient { get; }

    public override string MapToInternalPath(string hostPath) {
        return hostPath;
    }

    public override IShellWorker CreateShellWorker(ShellScript shellScript, Stream? inputStream, CancellationToken cancellationToken = default) {
        return new HttpShellWorker(HttpClient, shellScript, inputStream, cancellationToken);
    }
}
