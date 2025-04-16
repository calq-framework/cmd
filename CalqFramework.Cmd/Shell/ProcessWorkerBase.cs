using System.Diagnostics;

namespace CalqFramework.Cmd.Shell {
    public abstract class ProcessWorkerBase : ShellWorkerBase {
        private bool _disposed;

        private AutoTerminateProcess _process = null!; // initialized via Initialize

        public ProcessWorkerBase(ShellScript shellScript, Stream? inputStream, CancellationToken cancellationToken = default) : base(shellScript, inputStream, cancellationToken) {
        }

        public override Stream StandardOutput { get => _process.StandardOutput.BaseStream; }

        protected override int CompletionCode => _process.ExitCode;

        protected override void Dispose(bool disposing) {
            if (!_disposed) {
                _process.Dispose();

                _disposed = true;
            }
            base.Dispose(disposing);
        }

        protected override async Task<string> ReadErrorMessageAsync() {
            return await _process.StandardError.ReadToEndAsync();
        }

        protected override async Task<TextWriter?> Initialize(ShellScript shellScript, bool redirectInput) {
            var processExecutionInfo = GetProcessExecutionInfo(ShellScript.WorkingDirectory, ShellScript.Script);

            _process = new AutoTerminateProcess() {
                StartInfo = new ProcessStartInfo {
                    WorkingDirectory = ShellScript.WorkingDirectory,
                    FileName = processExecutionInfo.FileName,
                    RedirectStandardInput = redirectInput,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Arguments = processExecutionInfo.Arguments,
                },
                EnableRaisingEvents = true
            };

            _process.Exited += (s, e) => {
                RelayInputTaskAbortCts.Cancel();
            };

            _process.Start();

            return redirectInput ? _process.StandardInput : null;
        }

        protected override async Task WaitForCompletionAsync() {
            await _process.WaitForExitAsync();
        }

        internal abstract ProcessExecutionInfo GetProcessExecutionInfo(string workingDirectory, string script);
    }
}
