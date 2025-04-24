namespace CalqFramework.Cmd.Shell {
    public abstract class ExecutionOutputStream : Stream {
        public override bool CanRead => InnerStream.CanRead;

        public override bool CanSeek => InnerStream.CanSeek;

        public override bool CanWrite => InnerStream.CanWrite;

        public override long Length => InnerStream.Length;

        public override long Position { get => InnerStream.Position; set => InnerStream.Position = value; }

        protected abstract Stream InnerStream { get; }

        public override void Flush() {
            InnerStream.Flush();
        }

        public abstract long GetErrorCode();

        public abstract Task<long> GetErrorCodeAsync();

        public override int Read(byte[] buffer, int offset, int count) {
            int bytesRead = TryRead(buffer, offset, count);

            if (bytesRead == 0) {
                var errorCode = GetErrorCode();

                if (errorCode != 0) {
                    throw new ShellScriptExecutionException(errorCode, $"Error code: {errorCode}");
                }
            }

            return bytesRead;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
            int bytesRead = await TryReadAsync(buffer, offset, count, cancellationToken);

            if (bytesRead == 0) {
                var errorCode = await GetErrorCodeAsync();

                if (errorCode != 0) {
                    throw new ShellScriptExecutionException(errorCode, $"Error code: {errorCode}");
                }
            }

            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin) {
            return InnerStream.Seek(offset, origin);
        }

        public override void SetLength(long value) {
            InnerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count) {
            InnerStream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                InnerStream.Dispose();
            }
            base.Dispose(disposing);
        }

        protected abstract int TryRead(byte[] buffer, int offset, int count);

        protected abstract Task<int> TryReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
    }
}
