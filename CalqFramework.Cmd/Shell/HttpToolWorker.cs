using System.Net;

namespace CalqFramework.Cmd.Shell;

public class HttpToolWorker(HttpClient httpClient, ShellScript shellScript, Stream? inputStream) : ShellWorkerBase(shellScript, inputStream) {
    private bool _disposed;
    private ShellWorkerOutputStream? _executionOutputStream;
    private readonly HttpClient _httpClient = httpClient;
    private HttpResponseMessage? _response;

    public override ShellWorkerOutputStream StandardOutput => _executionOutputStream!;

    private HttpStatusCode? StatusCode => _response!.StatusCode; // TODO separate error handler

    public override async Task<string> ReadErrorMessageAsync(CancellationToken cancellationToken = default) {
        return await Task.FromResult("");
    }

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
        var request = new HttpRequestMessage {
            Version = new Version(2, 0)
        };
        request.Headers.Add("Script", shellScript.Script);

        request.Method = HttpMethod.Post;
        if (InputStream != null) {
            request.Content = new StreamContent(InputStream!);
        }

        _response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        Stream responseContentStream = await _response.Content.ReadAsStreamAsync(cancellationToken);
        _executionOutputStream = new HttpToolOutputStream(responseContentStream);
    }
}
