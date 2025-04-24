using System.Diagnostics;

namespace CalqFramework.Cmd.Shell {
    public class ProcessOutputStream : ExecutionOutputStream {
        private readonly Stream _innerStream;
        private readonly Process _process;

        public ProcessOutputStream(Process process) {
            _process = process;
            _innerStream = process.StandardOutput.BaseStream;
        }

        protected override Stream InnerStream => _innerStream;

        public override long GetErrorCode() {
            _process.WaitForExit();
            return _process.ExitCode;
        }

        public override async Task<long> GetErrorCodeAsync() {
            await _process.WaitForExitAsync();
            return _process.ExitCode;
        }

        protected override int TryRead(byte[] buffer, int offset, int count) {
            return _innerStream.Read(buffer, offset, count);
        }

        protected override async Task<int> TryReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
            return await _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
        }
    }
}
