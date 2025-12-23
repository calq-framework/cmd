using System.Diagnostics;

namespace CalqFramework.Cmd.Shell {

    public class ProcessOutputStream(Process process, Task relayInputTask, IShellWorker shellWorker) : ShellWorkerOutputStream(shellWorker) {
        private readonly Stream _innerStream = process.StandardOutput.BaseStream;
        private readonly Process _process = process;
        private readonly Task _realyInputTask = relayInputTask;

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
            if (_realyInputTask.IsFaulted) {
                throw _realyInputTask.Exception;
            }
            return _innerStream.Read(buffer, offset, count);
        }

        protected override async Task<int> TryReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
            if (_realyInputTask.IsFaulted) {
                throw _realyInputTask.Exception;
            }
            return await _innerStream.ReadAsync(buffer.AsMemory(offset, count), cancellationToken);
        }
    }
}
