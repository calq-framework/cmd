using CalqFramework.Cmd.SystemProcess;
using System.Diagnostics;

namespace CalqFramework.Cmd.Shell {
    public class ShellWorker : IDisposable {
        private bool _disposed;

        private AutoTerminateProcess _process;

        private Task RelayInputTask;

        public ShellWorker(ProcessExecutionInfo processExecutionInfo, IProcessStartConfiguration processStartConfiguration, CancellationToken cancellationToken = default) {
            ProcessExecutionInfo = processExecutionInfo;
            ProcessStartConfiguration = processStartConfiguration;

            _process = new AutoTerminateProcess() {
                StartInfo = new ProcessStartInfo {
                    WorkingDirectory = processStartConfiguration.WorkingDirectory,
                    FileName = processExecutionInfo.FileName,
                    RedirectStandardInput = true, // TODO false when null input
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Arguments = processExecutionInfo.Arguments,
                },
                EnableRaisingEvents = true
            };

            var relayInputTaskAbortCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _process.Exited += (s, e) => {
                relayInputTaskAbortCts.Cancel();
            };

            _process.Start();

            RelayInputTask = Task.Run(async () => await StreamUtils.RelayInput(_process.StandardInput, processStartConfiguration.In, processStartConfiguration.InWriter, cancellationToken)).WaitAsync(relayInputTaskAbortCts.Token); // input reading may lock thread
        }

        public ShellWorker? PipedWorker { get; init; }
        public TextReader StandardOutput { get => _process.StandardOutput; }
        private ProcessExecutionInfo ProcessExecutionInfo { get; }

        private IProcessStartConfiguration ProcessStartConfiguration { get; }
        public void Dispose() {
            if (!_disposed) {
                PipedWorker?.Dispose();
                _process.Dispose();

                _disposed = true;
            }
        }

        public async Task WaitForSuccess() {
            if (PipedWorker != null) {
                await PipedWorker.WaitForSuccess();
            }
            await _process.WaitForExitAsync();

            try {
                await RelayInputTask;
            } catch (TaskCanceledException) { } // triggered by relayInputTaskAbortCts which should be ignored

            ProcessStartConfiguration.ErrorHandler.AssertSuccess(_process.ExitCode, await _process.StandardError.ReadToEndAsync());
        }
    }
}
