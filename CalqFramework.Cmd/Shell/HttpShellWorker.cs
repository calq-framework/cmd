using CalqFramework.Cmd;
using CalqFramework.Cmd.Shell;
using System.Net;

public class HttpShellWorker : ShellWorkerBase {
    private ShellWorkerOutputStream? _executionOutputStream;
    private bool _disposed;

    private HttpResponseMessage? _response;
    private HttpClient _httpClient;
    public HttpShellWorker(HttpClient httpClient, ShellScript shellScript, Stream? inputStream) : base(shellScript, inputStream) {
        _httpClient = httpClient;
    }

    public override ShellWorkerOutputStream StandardOutput => _executionOutputStream!;

    private HttpStatusCode? StatusCode => _response!.StatusCode; // TODO separate error handler

    protected override void Dispose(bool disposing) {
        if (!_disposed) {
            if (disposing) {
                _response?.Dispose();
                _executionOutputStream?.Dispose();
            }
            _disposed = true;
        }
        base.Dispose(disposing);
    }

    protected override async Task InitializeAsync(ShellScript shellScript, CancellationToken cancellationToken = default) {
        var request = new HttpRequestMessage();
        request.Version = new Version(2, 0);
        request.Headers.Add("Script", shellScript.Script);

        request.Method = HttpMethod.Post;
        if (InputStream != null) {
            request.Content = new StreamContent(InputStream!);
        }

        _response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        var responseContentStream = await _response.Content.ReadAsStreamAsync(cancellationToken);
        _executionOutputStream = new HttpShellOutputStream(responseContentStream);
    }

    public override async Task<string> ReadErrorMessageAsync(CancellationToken cancellationToken = default) {
        return await Task.FromResult("");
    }
}
