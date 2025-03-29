using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd.TerminalComponents.ShellCommandComponents;
using System.Diagnostics;

namespace CalqFramework.Cmd {

    [DebuggerDisplay("{Script}")]
    public class ShellCommand {
        public ShellCommand(IShell shell, string script) {
            Shell = shell;
            Script = script;
        }

        public IShellCommandPostprocessor ShellCommandPostprocessor { get; init; } = new ShellCommandPostprocessor();
        private ShellCommand? PipedShellCommand { get; init; }
        public IShellCommandStartConfiguration ShellCommandStartConfiguration { get; init; } = new ShellCommandStartConfiguration();
        private string Script { get; }
        private IShell Shell { get; }

        public static implicit operator string(ShellCommand obj) {
            return obj.GetOutput();
        }

        public static ShellCommand operator |(ShellCommand a, ShellCommand b) {
            var c = new ShellCommand(b.Shell, b.Script) {
                ShellCommandStartConfiguration = b.ShellCommandStartConfiguration,
                PipedShellCommand = a
            };

            return c;
        }

        public string GetOutput(CancellationToken cancellationToken = default) {
            return GetOutputAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task<string> GetOutputAsync(CancellationToken cancellationToken = default) {
            using var worker = Start(cancellationToken);
            var outputWriter = new StringWriter();
            await RunAsync(outputWriter, worker, cancellationToken);
            var output = outputWriter.ToString();
            return ShellCommandPostprocessor.ProcessOutput(output);
        }

        public void Run(TextWriter outputWriter, CancellationToken cancellationToken = default) {
            RunAsync(outputWriter, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task RunAsync(TextWriter outputWriter, CancellationToken cancellationToken = default) {
            using var worker = Start(cancellationToken);
            await RunAsync(outputWriter, worker, cancellationToken);
        }

        public ShellWorkerBase Start(CancellationToken cancellationToken = default) {
            TextReader inputReader;
            ShellWorkerBase? pipedProcess = null;
            if (PipedShellCommand != null) {
                pipedProcess = PipedShellCommand.Start();
                inputReader = pipedProcess.StandardOutput;
            } else {
                inputReader = ShellCommandStartConfiguration.In;
            }

            var worker = Shell.CreateShellWorker(Script, new ShellCommandStartConfiguration(ShellCommandStartConfiguration) { In = inputReader }, pipedProcess, cancellationToken);
            return worker;
        }

        public override string ToString() {
            return GetOutput();
        }

        private async Task RunAsync(TextWriter outputWriter, ShellWorkerBase worker, CancellationToken cancellationToken = default) {
            var relayOutputCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var relayOutputTask = StreamUtils.RelayStream(worker.StandardOutput, outputWriter, relayOutputCts.Token);

            try {
                await worker.WaitForSuccess();
            } catch {
                relayOutputCts.Cancel();
                throw;
            }

            await relayOutputTask;

            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
