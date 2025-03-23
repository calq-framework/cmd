using System.Collections.Concurrent;
using System.Diagnostics;

namespace CalqFramework.Cmd.SystemProcess {
    public class RunnableProcess : Process {
        private static ConcurrentDictionary<Process, byte> _runningProcesses = new ConcurrentDictionary<Process, byte>();
        private bool _disposed;

        static RunnableProcess() {
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        }

        public RunnableProcess() {
            _disposed = false;
            HasStarted = false;
            InOutStreamCts = null;
        }

        private bool HasStarted { get; set; }

        public RunnableProcess? PipedProcess { get; init; }

        private CancellationTokenSource? InOutStreamCts { get; set; }

        private IProcessErrorHandler ErrorHandler { get; set; } = new ProcessErrorHandler();

        public async Task Run(ProcessExecutionInfo processExecutionInfo, IProcessRunConfiguration processRunConfiguration, CancellationToken cancellationToken = default) {
            var relayInputTask = StartCore(processExecutionInfo, processRunConfiguration, cancellationToken);

            var relayOutputTask = RelayStream(StandardOutput, processRunConfiguration.Out, InOutStreamCts.Token);

            await WaitForExitAsync(cancellationToken);

            try {
                await relayInputTask;
            } catch (TaskCanceledException) { } // triggered by relayInputTaskAbortCts which should be ignored

            await relayOutputTask;

            cancellationToken.ThrowIfCancellationRequested();

            await WaitForSuccess();
        }

        public async Task WaitForSuccess() {
            if (PipedProcess != null) {
                await PipedProcess.WaitForSuccess();
            }
            await WaitForExitAsync();
            ErrorHandler.AssertSuccess(ExitCode, await StandardError.ReadToEndAsync());
        }

        public void Start(ProcessExecutionInfo processExecutionInfo, IProcessStartConfiguration processStartConfiguration, CancellationToken cancellationToken = default) {
            _ = StartCore(processExecutionInfo, processStartConfiguration, cancellationToken);
        }

        protected override void Dispose(bool disposing) {
            if (!_disposed) {
                if (HasStarted && !HasExited) {
                    Kill(true); // killing already killed process shouldn't throw

                    if (InOutStreamCts != null) {
                        InOutStreamCts.Cancel();
                    }
                }

                if (disposing) {
                    PipedProcess?.Dispose();
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        private static void OnProcessExit(object? sender, EventArgs e) {
            foreach (var process in _runningProcesses.Keys) {
                try {
                    if (!process.HasExited) {
                        process.Kill(true);
                    }
                } catch (Exception) { }
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

        private Task StartCore(ProcessExecutionInfo processExecutionInfo, IProcessStartConfiguration processStartConfiguration, CancellationToken cancellationToken = default) {
            ErrorHandler = processStartConfiguration.ErrorHandler;

            var psi = new ProcessStartInfo {
                WorkingDirectory = processStartConfiguration.WorkingDirectory,
                FileName = processExecutionInfo.FileName,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = processExecutionInfo.Arguments,
            };

            StartInfo = psi;

            InOutStreamCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var relayInputTaskAbortCts = CancellationTokenSource.CreateLinkedTokenSource(InOutStreamCts.Token);

            EnableRaisingEvents = true;
            Exited += (s, e) => {
                relayInputTaskAbortCts.Cancel();
                _ = _runningProcesses.TryRemove(this, out _);
            };

            _ = _runningProcesses.TryAdd(this, 0);

            HasStarted = Start();

            return Task.Run(async () => await RelayInput(StandardInput, processStartConfiguration.In, processStartConfiguration.InWriter, InOutStreamCts.Token)).WaitAsync(relayInputTaskAbortCts.Token); // input reading may lock thread
        }
    }
}
