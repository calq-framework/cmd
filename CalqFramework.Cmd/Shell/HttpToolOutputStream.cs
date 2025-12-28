namespace CalqFramework.Cmd.Shell {

    /// <summary>
    /// HTTP-based output stream that wraps HTTP response content for shell operations.
    /// Handles HTTP protocol errors and converts them to shell worker errors.
    /// </summary>
    public class HttpToolOutputStream(Stream responseContentStream, IShellWorker shellWorker) : ShellWorkerOutputStream(shellWorker) {
        private readonly Stream _innerStream = responseContentStream;
        private Error _error = new(0, null);

        /// <summary>
        /// The underlying HTTP response content stream.
        /// </summary>
        protected override Stream InnerStream => _innerStream;

        /// <summary>
        /// Gets the current error state from HTTP operations.
        /// </summary>
        /// <returns>Error information including HTTP error codes</returns>
        protected override Error GetError() {
            return _error;
        }

        /// <summary>
        /// Asynchronously gets the current error state from HTTP operations.
        /// </summary>
        /// <returns>Task containing error information including HTTP error codes</returns>
        protected override Task<Error> GetErrorAsync() {
            return Task.FromResult(_error);
        }

        /// <summary>
        /// Attempts to read data from the HTTP response stream, capturing HTTP protocol errors.
        /// </summary>
        /// <returns>Number of bytes read, or 0 if an error occurred</returns>
        protected override int TryRead(byte[] buffer, int offset, int count) {
            try {
                int bytesRead = _innerStream.Read(buffer, offset, count);
                return bytesRead;
            } catch (HttpProtocolException ex) {
                _error = new Error(ex.ErrorCode, ex);
                return 0;
            }
        }

        /// <summary>
        /// Asynchronously attempts to read data from the HTTP response stream, capturing HTTP protocol errors.
        /// </summary>
        /// <returns>Task containing the number of bytes read, or 0 if an error occurred</returns>
        protected override async Task<int> TryReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
            try {
                int bytesRead = await _innerStream.ReadAsync(buffer.AsMemory(offset, count), cancellationToken);
                return bytesRead;
            } catch (HttpProtocolException ex) {
                _error = new Error(ex.ErrorCode, ex);
                return 0;
            }
        }
    }
}
