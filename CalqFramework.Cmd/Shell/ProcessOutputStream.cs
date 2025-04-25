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

        protected override Error GetError() {
            _process.WaitForExit();
            return new Error(_process.ExitCode, null);
        }

        protected override async Task<Error> GetErrorAsync() {
            await _process.WaitForExitAsync();
            return new Error(_process.ExitCode, null);
        }

        protected override int TryRead(byte[] buffer, int offset, int count) {
            return _innerStream.Read(buffer, offset, count);
        }

        protected override async Task<int> TryReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
            return await _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
        }
    }
}
