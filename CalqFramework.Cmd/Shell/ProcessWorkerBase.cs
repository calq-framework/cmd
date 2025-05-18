﻿using System.Diagnostics;

namespace CalqFramework.Cmd.Shell {

    public abstract class ProcessWorkerBase(ShellScript shellScript, Stream? inputStream) : ShellWorkerBase(shellScript, inputStream) {
        private bool _disposed;
        private AutoTerminateProcess _process = null!;
        private ProcessOutputStream? _processOutputStream;

        public override ShellWorkerOutputStream StandardOutput { get => _processOutputStream!; }

        public override async Task<string> ReadErrorMessageAsync(CancellationToken cancellationToken = default) {
            return await _process.StandardError.ReadToEndAsync(cancellationToken);
        }

        internal abstract ProcessExecutionInfo GetProcessExecutionInfo(string workingDirectory, string script);

        protected override void Dispose(bool disposing) {
            if (!_disposed) {
                _process.Dispose();

                _disposed = true;
            }
            base.Dispose(disposing);
        }

        protected override Task InitializeAsync(ShellScript shellScript, CancellationToken cancellationToken = default) {
            ProcessExecutionInfo processExecutionInfo = GetProcessExecutionInfo(ShellScript.WorkingDirectory, ShellScript.Script);

            _process = new AutoTerminateProcess() {
                StartInfo = new ProcessStartInfo {
                    WorkingDirectory = ShellScript.WorkingDirectory,
                    FileName = processExecutionInfo.FileName,
                    RedirectStandardInput = InputStream != null,
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

            Task relayInputTask = Task.CompletedTask;
            if (InputStream != null) {
                relayInputTask = Task.Run(async () => await StreamUtils.RelayInput(_process.StandardInput!, new StreamReader(InputStream!), relayInputTaskAbortCts.Token), cancellationToken).WaitAsync(relayInputTaskAbortCts.Token); // input reading may lock thread
            }

            _processOutputStream = new ProcessOutputStream(_process, relayInputTask);

            return Task.CompletedTask;
        }
    }
}
