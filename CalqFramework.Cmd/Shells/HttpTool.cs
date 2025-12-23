using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd.Shells;

public class HttpTool(HttpClient httpClient) : ShellBase {
    public HttpClient HttpClient { get; } = httpClient;

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
