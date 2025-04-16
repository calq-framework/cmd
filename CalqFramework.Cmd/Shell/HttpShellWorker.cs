using CalqFramework.Cmd;
using CalqFramework.Cmd.Shell;
using System.Net;

public class HttpShellWorker : ShellWorkerBase {
    private Stream? _content;
    private bool _disposed;

    private HttpResponseMessage? _response;
    private HttpClient HttpClient;
    public HttpShellWorker(HttpClient httpClient, ShellScript shellScript, Stream? inputStream, CancellationToken cancellationToken = default) : base(shellScript, inputStream, cancellationToken) {
        HttpClient = httpClient;
    }

    public override StreamReader StandardOutput => new StreamReader(_content!);

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

    protected override async Task<TextWriter?> Initialize(ShellScript shellScript, bool redirectInput) {
        var request = new HttpRequestMessage();
        request.Headers.Add("Script", shellScript.Script);

        request.Method = HttpMethod.Post;
        if (redirectInput) {
            //request.Content = new TextReaderHttpContent(InputStream!);
            //request.Content = new StringContent(InputStream.ReadToEnd()!);
            //request.Content = new StreamContent(new StringStream("hello world"));

            //string test = "hello world";

            // convert string to stream

            request.Content = new StreamContent(InputStream!);
            //request.Content = new StreamContent(Console.OpenStandardInput());
        } else {
            request.Content = new StringContent("");
        }

        _response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, RelayInputTaskAbortCts.Token);

        _content = await _response.Content.ReadAsStreamAsync();

        return null;
    }

    protected override async Task<string> ReadErrorMessageAsync() {
        return "";
    }
    protected override async Task WaitForCompletionAsync() {
        // do nothing
    }
}
