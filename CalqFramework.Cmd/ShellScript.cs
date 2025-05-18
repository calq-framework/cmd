using System.Diagnostics;
using System.Text;
using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd {

    [DebuggerDisplay("{Script}")]
    public class ShellScript(IShell shell, string script) {
        public static AsyncLocal<string> LocalWorkingDirectory = new();

        public ShellScript? PipedShellScript { get; private init; }
        public string Script { get; internal set; } = script;
        public IShell Shell { get; internal set; } = shell;
        public string WorkingDirectory { get; init; } = LocalWorkingDirectory.Value ?? Environment.CurrentDirectory;

        public static implicit operator string(ShellScript obj) {
            return obj.Evaluate();
        }

        public static ShellScript operator |(ShellScript a, ShellScript b) {
            var c = new ShellScript(b.Shell, b.Script) {
                WorkingDirectory = b.WorkingDirectory,
                PipedShellScript = a
            };

            return c;
        }

        public string Evaluate(CancellationToken cancellationToken = default) {
            return EvaluateAsync(cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public string Evaluate(Stream? inputStream, CancellationToken cancellationToken = default) {
            return EvaluateAsync(inputStream, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task<string> EvaluateAsync(Stream? inputStream, CancellationToken cancellationToken = default) {
            using IShellWorker worker = await StartAsync(inputStream, cancellationToken);
            using var reader = new StreamReader(worker.StandardOutput);

            var sb = new StringBuilder();
            char[] buffer = new char[4096];
            try {
                while (true) {
                    int charsRead = await reader.ReadAsync(buffer, 0, buffer.Length);
                    if (charsRead == 0) {
                        break;
                    }
                    sb.Append(buffer, 0, charsRead);
                }
                await worker.EnsurePipeIsCompletedAsync(cancellationToken);
            } catch (ShellWorkerException ex) {
                throw await Shell.ExceptionFactory.CreateAsync(this, worker, ex, sb.ToString(), cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();

            return Shell.Postprocessor.ProcessOutput(sb.ToString());
        }

        public async Task<string> EvaluateAsync(CancellationToken cancellationToken = default) {
            return await EvaluateAsync(null, cancellationToken);
        }

        public void Run(Stream outputStream, CancellationToken cancellationToken = default) {
            RunAsync(outputStream, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void Run(Stream? inputStream, Stream outputStream, CancellationToken cancellationToken = default) {
            RunAsync(inputStream, outputStream, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task RunAsync(Stream outputStream, CancellationToken cancellationToken = default) {
            await RunAsync(Shell.In, outputStream, cancellationToken);
        }

        public async Task RunAsync(Stream? inputStream, Stream outputStream, CancellationToken cancellationToken = default) {
            using IShellWorker worker = await StartAsync(inputStream, cancellationToken);
            var relayOutputCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            try {
                await StreamUtils.RelayStream(worker.StandardOutput, outputStream, relayOutputCts.Token);
                await worker.EnsurePipeIsCompletedAsync(cancellationToken);
            } catch (ShellWorkerException ex) {
                throw await Shell.ExceptionFactory.CreateAsync(this, worker, ex, null, cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        public async Task<IShellWorker> StartAsync(CancellationToken cancellationToken = default) {
            IShellWorker worker = Shell.CreateShellWorker(this, Shell.In);
            await worker.StartAsync(cancellationToken);
            return worker;
        }

        public async Task<IShellWorker> StartAsync(Stream? inputStream, CancellationToken cancellationToken = default) {
            IShellWorker worker = Shell.CreateShellWorker(this, inputStream);
            await worker.StartAsync(cancellationToken);
            return worker;
        }

        public override string ToString() {
            return Evaluate();
        }
    }
}
