using System.Buffers;
using System.Net;

namespace CalqFramework.Cmd.Shell;

/// <summary>
///     HTTP-based shell worker for distributed command execution.
///     Communicates with HTTP servers via HTTP/2, supports streaming and error handling.
///     Used by HttpTool and PythonTool for remote execution.
/// </summary>
public class HttpToolWorker(
    HttpClient httpClient,
    ShellScript shellScript,
    Stream? inputStream,
    bool disposeOnCompletion = true) : ShellWorkerBase(shellScript, inputStream, disposeOnCompletion) {
    private readonly HttpClient _httpClient = httpClient;
    private bool _disposed;
    private HttpToolOutputStream? _executionOutputStream;
    private HttpResponseMessage? _response;

    public override ShellWorkerOutputStream StandardOutput => _executionOutputStream!;

    private HttpStatusCode? StatusCode => _response!.StatusCode;

    public override async Task<string> ReadErrorMessageAsync(CancellationToken cancellationToken = default) {
        try {
            byte[] buffer = ArrayPool<byte>.Shared.Rent(1024);
            try {
                while (await _executionOutputStream!.ReadAsync(buffer.AsMemory(0, 1024), cancellationToken) > 0) {
                }
            } finally {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return "";
        } catch (ShellWorkerException ex) when (ex.ErrorCode.HasValue && ex.ErrorCode.Value != 0) {
            long errorCode = ex.ErrorCode.Value;

            try {
                HttpRequestMessage request = new() {
                    Version = new Version(2, 0),
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("/read_error_message", UriKind.Relative),
                    Content = new StringContent("")
                };

                // Cast to unsigned 32-bit for consistency with Python's 32-bit unsigned range
                uint unsignedErrorCode = (uint)errorCode;
                request.Headers.Add("error_code", unsignedErrorCode.ToString());

                using HttpResponseMessage errorResponse = await _httpClient.SendAsync(request, cancellationToken);

                if (errorResponse.StatusCode == HttpStatusCode.NotFound) {
                    string notFoundMessage = await errorResponse.Content.ReadAsStringAsync(cancellationToken);
                    return $"Error occurred (code: {errorCode}), but error details not available: {notFoundMessage}";
                }

                errorResponse.EnsureSuccessStatusCode();

                return await errorResponse.Content.ReadAsStringAsync(cancellationToken);
            } catch (HttpRequestException retrievalEx) {
                return
                    $"Error occurred (code: {errorCode}), but could not retrieve detailed error message: {retrievalEx.Message}";
            } catch (TaskCanceledException retrievalEx) when (retrievalEx.InnerException is TimeoutException) {
                return
                    $"Error occurred (code: {errorCode}), but timed out retrieving detailed error message: {retrievalEx.Message}";
            }
        }
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

    protected override async Task InitializeAsync(ShellScript shellScript,
        CancellationToken cancellationToken = default) {
        HttpRequestMessage request = new() {
            Version = new Version(2, 0)
        };
        request.Headers.Add("Script", shellScript.Script);

        request.Method = HttpMethod.Post;
        if (InputStream != null) {
            request.Content = new StreamContent(InputStream!);
        }

        _response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        Stream responseContentStream = await _response.Content.ReadAsStreamAsync(cancellationToken);
        _executionOutputStream = new HttpToolOutputStream(responseContentStream, this);
    }
}
