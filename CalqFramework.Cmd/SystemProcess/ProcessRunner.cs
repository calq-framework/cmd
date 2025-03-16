using System.Diagnostics;

namespace CalqFramework.Cmd.SystemProcess {
    internal class ProcessRunner : IDisposable {
        private bool _disposed;

        public ProcessRunner() {
            _disposed = false;
            HasStarted = false;
            InOutStreamCts = null;
            Process = null;
        }

        ~ProcessRunner() {
            Dispose(false);
        }

        private bool HasStarted { get; set; }
        private CancellationTokenSource? InOutStreamCts { get; set; }
        private Process? Process { get; set; }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task Run(ProcessRunInfo processRunInfo, IProcessRunConfiguration processRunConfiguration, CancellationToken cancellationToken = default) {
            var psi = new ProcessStartInfo {
                WorkingDirectory = processRunConfiguration.WorkingDirectory,
                FileName = processRunInfo.FileName,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = processRunInfo.Arguments
            };

            Process = new Process() {
                StartInfo = psi
            };

            HasStarted = Process.Start();

            InOutStreamCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var relayInputTaskAbortCts = CancellationTokenSource.CreateLinkedTokenSource(InOutStreamCts.Token);

            var relayInputTask = Task.Run(async () => await RelayInput(Process.StandardInput, processRunConfiguration.In, processRunConfiguration.Out, InOutStreamCts.Token)).WaitAsync(relayInputTaskAbortCts.Token); // input reading may lock threadb

            var relayOutputTask = RelayStream(Process.StandardOutput, processRunConfiguration.Out, InOutStreamCts.Token);

            var errorWriter = new StringWriter();
            var relayErrorTask = RelayStream(Process.StandardError, errorWriter, InOutStreamCts.Token);

            await Process.WaitForExitAsync(cancellationToken);

            relayInputTaskAbortCts.Cancel();
            try {
                await relayInputTask;
            } catch (TaskCanceledException) { } // triggered by relayInputTaskAbortCts which should be ignored

            await relayOutputTask;
            await relayErrorTask;

            cancellationToken.ThrowIfCancellationRequested();

            var error = errorWriter.ToString();

            processRunConfiguration.ErrorHandler.AssertSuccess(processRunInfo, processRunConfiguration, Process, error);
        }

        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                if (Process != null) {
                    if (HasStarted && !Process.HasExited) {
                        Process.Kill(true); // killing already killed process shouldn't throw

                        if (InOutStreamCts != null) {
                            InOutStreamCts.Cancel();
                        }
                    }


                    if (disposing) {
                        Process.Dispose();
                    }
                }

                _disposed = true;
            }
        }

        private static async Task RelayInput(StreamWriter processInput, TextReader inputReader, TextWriter outputWriter, CancellationToken cancellationToken) {
            try {
                if (Environment.UserInteractive && ReferenceEquals(inputReader, Console.OpenStandardInput())) {
                    while (!cancellationToken.IsCancellationRequested) {
                        if (Console.KeyAvailable) {
                            var keyChar = Console.ReadKey(true).KeyChar;
                            if (keyChar == '\r') { // windows enterkey is \r and deletes what was typed because of that
                                keyChar = '\n';
                            }
                            outputWriter.Write(keyChar);
                            processInput.Write(keyChar);
                        }

                        await Task.Delay(1);
                    }
                } else {
                    // TODO relay block by block
                    var buffer = new char[1];

                    while (!cancellationToken.IsCancellationRequested) {
                        var bytesRead = await inputReader.ReadAsync(buffer, cancellationToken);
                        var keyChar = buffer[0];
                        if (bytesRead == 0 || keyChar == -1 || keyChar == '\uffff') {
                            break;
                        }
                        processInput.Write(keyChar);

                        await Task.Delay(1);
                    }
                }
            } finally {
                processInput.Close(); // in case input stream reached EOF close input stream to signal EOF to the process
            }
        }

        private static async Task RelayStream(StreamReader reader, TextWriter writer, CancellationToken cancellationToken) {
            var bufferArray = new char[4096];

            while (!cancellationToken.IsCancellationRequested) {
                bool isRead = false;
                int bytesRead = 0;
                try {
                    Array.Clear(bufferArray);
                    var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(20));
                    bytesRead = await reader.ReadAsync(bufferArray, cancellationTokenSource.Token);
                    isRead = true;
                } catch (OperationCanceledException) {
                    isRead = false;
                    bytesRead = Array.IndexOf(bufferArray, '\0');
                    if (bytesRead > 0) {
                        await writer.WriteAsync(bufferArray, 0, bytesRead);
                        await writer.FlushAsync();
                        continue;
                    }
                }

                if (isRead && bytesRead == 0) {
                    break;
                }

                if (bytesRead > 0) {
                    await writer.WriteAsync(bufferArray, 0, bytesRead);
                }

                await Task.Delay(1);
            }

            await writer.FlushAsync();
        }
    }
}