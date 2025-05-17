
namespace CalqFramework.Cmd.Shell {
    public class HttpToolOutputStream : ShellWorkerOutputStream {
        private readonly Stream _innerStream;
        private Error _error = new Error(0, null);

        public HttpToolOutputStream(Stream responseContentStream) {
            _innerStream = responseContentStream;
        }

        protected override Stream InnerStream => _innerStream;

        protected override Error GetError() {
            return _error;
        }

        protected override Task<Error> GetErrorAsync() {
            return Task.FromResult(_error);
        }

        protected override int TryRead(byte[] buffer, int offset, int count) {
            try {
                int bytesRead = _innerStream.Read(buffer, offset, count);
                return bytesRead;
            } catch (HttpProtocolException ex) {
                _error = new Error(ex.ErrorCode, ex);
                return 0;
            }
        }

        protected override async Task<int> TryReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
            try {
                int bytesRead = await _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
                return bytesRead;
            } catch (HttpProtocolException ex) {
                _error = new Error(ex.ErrorCode, ex);
                return 0;
            }
        }
    }
}
