using System.Diagnostics;

namespace CalqFramework.Cmd.Shell {
    internal class ProcessStream : Stream {
        private readonly Stream _inner;
        private readonly Process _process;

        public ProcessStream(Process process) {
            _process = process;
            _inner = process.StandardOutput.BaseStream;
        }

        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => _inner.CanSeek;
        public override bool CanWrite => _inner.CanWrite;
        public override long Length => _inner.Length;
        public override long Position {
            get => _inner.Position;
            set => _inner.Position = value;
        }

        public override void Flush() {
            _inner.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin) {
            return _inner.Seek(offset, origin);
        }

        public override void SetLength(long value) {
            _inner.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count) {
            _inner.Write(buffer, offset, count);
        }

        public override int Read(byte[] buffer, int offset, int count) {
            int bytesRead = _inner.Read(buffer, offset, count);

            if (bytesRead == 0) {
                _process.WaitForExit();
            }

            return bytesRead;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
            int bytesRead = await _inner.ReadAsync(buffer, offset, count, cancellationToken);

            if (bytesRead == 0d) {
                await _process.WaitForExitAsync();
            }

            return bytesRead;
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                _inner.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
