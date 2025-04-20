using CalqFramework.Cmd;
using CalqFramework.Cmd.Shell;
using System.Net;

public class HttpShellWorker : ShellWorkerBase {
    private Stream? _content;
    private bool _disposed;

    private HttpResponseMessage? _response;
    private HttpClient HttpClient;
    public HttpShellWorker(HttpClient httpClient, ShellScript shellScript, Stream? inputStream) : base(shellScript, inputStream) {
        HttpClient = httpClient;
    }

    public override Stream StandardOutput => _content!;

    protected override int CompletionCode => (int)(_response?.StatusCode ?? 0);
    private HttpStatusCode? StatusCode => _response!.StatusCode; // TODO separate error handler

    protected override void Dispose(bool disposing) {
        if (!_disposed) {
            if (disposing) {
                _response?.Dispose();
                _content?.Dispose();
            }
            _disposed = true;
        }
        base.Dispose(disposing);
    }

    protected override async Task InitializeAsync(ShellScript shellScript, bool redirectInput, CancellationToken cancellationToken = default) {
        var request = new HttpRequestMessage();
        request.Version = new Version(2, 0);
        request.Headers.Add("Script", shellScript.Script);

        request.Method = HttpMethod.Post;
        if (redirectInput) {
            request.Content = new StreamContent(InputStream!);
        }

        _response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        _content = await _response.Content.ReadAsStreamAsync(cancellationToken);
    }

    protected override async Task<string> ReadErrorMessageAsync() {
        return "";
    }
    protected override async Task WaitForCompletionAsync() {
        // do nothing
    }
}
