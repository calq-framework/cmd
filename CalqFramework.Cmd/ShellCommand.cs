using CalqFramework.Cmd.Shell;
using System.Diagnostics;

namespace CalqFramework.Cmd {

    [DebuggerDisplay("{Script}")]
    public class ShellCommand {
        public static AsyncLocal<string> LocalWorkingDirectory = new();

        public ShellCommand(IShell shell, string script) {
            Shell = shell;
            Script = script;
            WorkingDirectory = LocalWorkingDirectory.Value ?? Environment.CurrentDirectory;
        }

        public ShellCommand? PipedShellCommand { get; private init; }
        public string Script { get; }
        public IShell Shell { get; }
        public string WorkingDirectory { get; init; }

        public static implicit operator string(ShellCommand obj) {
            return obj.Evaluate();
        }

        public static ShellCommand operator |(ShellCommand a, ShellCommand b) {
            var c = new ShellCommand(b.Shell, b.Script) {
                WorkingDirectory = b.WorkingDirectory,
                PipedShellCommand = a
            };

            return c;
        }

        public string Evaluate(CancellationToken cancellationToken = default) {
            return EvaluateAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public string Evaluate(TextReader inputReader, CancellationToken cancellationToken = default) {
            return EvaluateAsync(inputReader).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task<string> EvaluateAsync(TextReader inputReader, CancellationToken cancellationToken = default) {
            using var worker = Start(inputReader, cancellationToken);
            var outputWriter = new StringWriter();
            await RunAsync(outputWriter, worker, cancellationToken);
            var output = outputWriter.ToString();
            return Shell.Postprocessor.ProcessOutput(output);
        }

        public async Task<string> EvaluateAsync(CancellationToken cancellationToken = default) {
            using var worker = Start(cancellationToken);
            var outputWriter = new StringWriter();
            await RunAsync(outputWriter, worker, cancellationToken);
            var output = outputWriter.ToString();
            return Shell.Postprocessor.ProcessOutput(output);
        }

        public void Run(TextWriter outputWriter, CancellationToken cancellationToken = default) {
            RunAsync(outputWriter, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void Run(TextReader inputReader, TextWriter outputWriter, CancellationToken cancellationToken = default) {
            RunAsync(inputReader, outputWriter, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task RunAsync(TextWriter outputWriter, CancellationToken cancellationToken = default) {
            using var worker = Start(cancellationToken);
            await RunAsync(outputWriter, worker, cancellationToken);
        }
        public async Task RunAsync(TextReader inputReader, TextWriter outputWriter, CancellationToken cancellationToken = default) {
            using var worker = Start(inputReader, cancellationToken);
            await RunAsync(outputWriter, worker, cancellationToken);
        }

        public ShellWorkerBase Start(CancellationToken cancellationToken = default) {
            var worker = Shell.CreateShellWorker(this, cancellationToken);
            return worker;
        }

        public ShellWorkerBase Start(TextReader inputReader, CancellationToken cancellationToken = default) {
            var worker = Shell.CreateShellWorker(this, inputReader, cancellationToken);
            return worker;
        }

        public override string ToString() {
            return Evaluate();
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
