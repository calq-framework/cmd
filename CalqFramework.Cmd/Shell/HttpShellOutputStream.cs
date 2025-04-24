
namespace CalqFramework.Cmd.Shell {
    public class HttpShellOutputStream : ExecutionOutputStream {
        private readonly Stream _innerStream;
        private long _errorCode = 0;

        public HttpShellOutputStream(Stream responseContentStream) {
            _innerStream = responseContentStream;
        }

        protected override Stream InnerStream => _innerStream;

        public override long GetErrorCode() {
            return _errorCode;
        }

        public override Task<long> GetErrorCodeAsync() {
            return Task.FromResult(_errorCode);
        }

        protected override int TryRead(byte[] buffer, int offset, int count) {
            try {
                int bytesRead = _innerStream.Read(buffer, offset, count);
                return bytesRead;
            } catch (HttpProtocolException ex) {
                _errorCode = ex.ErrorCode;
                return 0;
            }
        }

        protected override async Task<int> TryReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
            try {
                int bytesRead = await _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
                return bytesRead;
            } catch (HttpProtocolException ex) {
                _errorCode = ex.ErrorCode;
                return 0;
            }
        }
    }
}
