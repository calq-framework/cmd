using System.Diagnostics;
using System.Text;
using System.Text.Json;
using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd {

    /// <summary>
    /// Represents a shell command that can be executed, piped, and chained.
    /// Supports the | operator for creating command pipelines that run in parallel.
    /// Provides unified interface for Process and HttpClient execution.
    /// </summary>

    [DebuggerDisplay("{Script}")]
    public class ShellScript(IShell shell, string script) {
        /// <summary>
        /// AsyncLocal storage for working directory, enabling thread/task isolation.
        /// </summary>
        public static AsyncLocal<string> LocalWorkingDirectory { get; set; } = new();

        /// <summary>
        /// Gets the previous shell script in a pipeline chain, if any.
        /// Used internally to manage command piping with the | operator.
        /// </summary>
        public ShellScript? PipedShellScript { get; private init; }
        
        /// <summary>
        /// Gets or sets the shell command text to execute.
        /// Contains the actual command string that will be run by the shell.
        /// </summary>
        public string Script { get; internal set; } = script;
        
        /// <summary>
        /// Gets or sets the shell implementation used to execute this script.
        /// Determines the execution environment (CommandLine, Bash, HttpTool, etc.).
        /// </summary>
        public IShell Shell { get; internal set; } = shell;
        
        /// <summary>
        /// Gets the working directory where the command will be executed.
        /// Defaults to the current AsyncLocal working directory or Environment.CurrentDirectory.
        /// </summary>
        public string WorkingDirectory { get; init; } = LocalWorkingDirectory.Value ?? Environment.CurrentDirectory;

        /// <summary>
        /// Implicitly converts a ShellScript to its evaluated string output.
        /// Executes the command and returns the result as a string.
        /// </summary>
        public static implicit operator string(ShellScript obj) {
            return obj.Evaluate();
        }

        /// <summary>
        /// Pipeline operator for chaining commands. Creates a new ShellScript with the left side piped to the right.
        /// Pipeline steps run in parallel for improved performance.
        /// </summary>
        public static ShellScript operator |(ShellScript a, ShellScript b) {
            var c = new ShellScript(b.Shell, b.Script) {
                WorkingDirectory = b.WorkingDirectory,
                PipedShellScript = a
            };

            return c;
        }

        /// <summary>
        /// Executes the shell script synchronously and returns the output as a string.
        /// Uses the shell's default input stream and processes the entire output.
        /// </summary>
        /// <returns>The complete command output as a string</returns>
        public string Evaluate(CancellationToken cancellationToken = default) {
            return EvaluateAsync(cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Executes the shell script synchronously with custom input and returns the output as a string.
        /// Allows providing input data to be fed to the command's stdin.
        /// </summary>
        /// <returns>The complete command output as a string</returns>
        public string Evaluate(Stream? inputStream, CancellationToken cancellationToken = default) {
            return EvaluateAsync(inputStream, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Executes the shell script asynchronously with custom input and returns the output as a string.
        /// Reads the entire output stream and processes it through the shell's postprocessor.
        /// </summary>
        /// <returns>Task containing the complete command output as a string</returns>
        public async Task<string> EvaluateAsync(Stream? inputStream, CancellationToken cancellationToken = default) {
            using IShellWorker worker = await StartAsync(inputStream, disposeOnCompletion: false, cancellationToken);
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

        /// <summary>
        /// Executes the shell script asynchronously and returns the output as a string.
        /// Uses the shell's default input stream and processes the entire output.
        /// </summary>
        /// <returns>Task containing the complete command output as a string</returns>
        public async Task<string> EvaluateAsync(CancellationToken cancellationToken = default) {
            return await EvaluateAsync(null, cancellationToken);
        }

        /// <summary>
        /// Executes the shell script synchronously and deserializes the JSON output to the specified type.
        /// Uses the shell's default input stream and processes the entire output.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the JSON output to</typeparam>
        /// <returns>The deserialized object of type T, or null if deserialization returns null</returns>
        /// <exception cref="JsonException">Thrown when the output is not valid JSON</exception>
        public T? Evaluate<T>(CancellationToken cancellationToken = default) {
            string json = Evaluate(cancellationToken);
            return JsonSerializer.Deserialize<T>(json);
        }

        /// <summary>
        /// Executes the shell script synchronously with custom input and deserializes the JSON output to the specified type.
        /// Allows providing input data to be fed to the command's stdin.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the JSON output to</typeparam>
        /// <param name="inputStream">Input stream to feed to the command's stdin</param>
        /// <returns>The deserialized object of type T, or null if deserialization returns null</returns>
        /// <exception cref="JsonException">Thrown when the output is not valid JSON</exception>
        public T? Evaluate<T>(Stream? inputStream, CancellationToken cancellationToken = default) {
            string json = Evaluate(inputStream, cancellationToken);
            return JsonSerializer.Deserialize<T>(json);
        }

        /// <summary>
        /// Executes the shell script asynchronously and deserializes the JSON output to the specified type.
        /// Uses the shell's default input stream and processes the entire output.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the JSON output to</typeparam>
        /// <returns>Task containing the deserialized object of type T, or null if deserialization returns null</returns>
        /// <exception cref="JsonException">Thrown when the output is not valid JSON</exception>
        public async Task<T?> EvaluateAsync<T>(CancellationToken cancellationToken = default) {
            string json = await EvaluateAsync(cancellationToken);
            return JsonSerializer.Deserialize<T>(json);
        }

        /// <summary>
        /// Executes the shell script asynchronously with custom input and deserializes the JSON output to the specified type.
        /// Allows providing input data to be fed to the command's stdin.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the JSON output to</typeparam>
        /// <param name="inputStream">Input stream to feed to the command's stdin</param>
        /// <returns>Task containing the deserialized object of type T, or null if deserialization returns null</returns>
        /// <exception cref="JsonException">Thrown when the output is not valid JSON</exception>
        public async Task<T?> EvaluateAsync<T>(Stream? inputStream, CancellationToken cancellationToken = default) {
            string json = await EvaluateAsync(inputStream, cancellationToken);
            return JsonSerializer.Deserialize<T>(json);
        }

        /// <summary>
        /// Executes the shell script synchronously and streams output to the specified stream.
        /// Uses the shell's default input stream.
        /// </summary>
        public void Run(Stream outputStream, CancellationToken cancellationToken = default) {
            RunAsync(outputStream, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Executes the shell script synchronously with custom input and streams output to the specified stream.
        /// Allows providing input data to be fed to the command's stdin.
        /// </summary>
        public void Run(Stream? inputStream, Stream outputStream, CancellationToken cancellationToken = default) {
            RunAsync(inputStream, outputStream, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Executes the shell script asynchronously and streams output to the specified stream.
        /// Uses the shell's default input stream.
        /// </summary>
        /// <returns>Task that completes when the command finishes execution</returns>
        public async Task RunAsync(Stream outputStream, CancellationToken cancellationToken = default) {
            await RunAsync(Shell.In, outputStream, cancellationToken);
        }

        /// <summary>
        /// Executes the shell script asynchronously with custom input and streams output to the specified stream.
        /// Provides full control over input/output streams for advanced scenarios.
        /// </summary>
        /// <returns>Task that completes when the command finishes execution</returns>
        public async Task RunAsync(Stream? inputStream, Stream outputStream, CancellationToken cancellationToken = default) {
            using IShellWorker worker = await StartAsync(inputStream, disposeOnCompletion: false, cancellationToken);
            var relayOutputCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            try {
                await worker.StandardOutput.CopyToAsync(outputStream, relayOutputCts.Token);
                await worker.EnsurePipeIsCompletedAsync(cancellationToken);
            } catch (ShellWorkerException ex) {
                throw await Shell.ExceptionFactory.CreateAsync(this, worker, ex, null, cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Starts the shell script asynchronously and returns a worker for stream control.
        /// Provides access to StandardOutput and error handling via ReadErrorMessageAsync.
        /// Uses disposeOnCompletion: true by default.
        /// </summary>
        /// <returns>A worker instance providing access to StandardOutput and error handling</returns>
        public async Task<IShellWorker> StartAsync(CancellationToken cancellationToken = default) {
            return await StartAsync(disposeOnCompletion: true, cancellationToken);
        }

        /// <summary>
        /// Starts the shell script asynchronously and returns a worker for stream control.
        /// Provides access to StandardOutput and error handling via ReadErrorMessageAsync.
        /// </summary>
        /// <returns>A worker instance providing access to StandardOutput and error handling</returns>
        public async Task<IShellWorker> StartAsync(bool disposeOnCompletion, CancellationToken cancellationToken = default) {
            IShellWorker worker = Shell.CreateShellWorker(this, Shell.In, disposeOnCompletion);
            await worker.StartAsync(cancellationToken);
            return worker;
        }

        /// <summary>
        /// Starts the shell script asynchronously with custom input and returns a worker for stream control.
        /// Allows providing input data and accessing StandardOutput for manual stream handling.
        /// Uses disposeOnCompletion: true by default.
        /// </summary>
        /// <returns>A worker instance providing access to StandardOutput and error handling</returns>
        public async Task<IShellWorker> StartAsync(Stream? inputStream, CancellationToken cancellationToken = default) {
            return await StartAsync(inputStream, disposeOnCompletion: true, cancellationToken);
        }

        /// <summary>
        /// Starts the shell script asynchronously with custom input and returns a worker for stream control.
        /// Allows providing input data and accessing StandardOutput for manual stream handling.
        /// </summary>
        /// <returns>A worker instance providing access to StandardOutput and error handling</returns>
        public async Task<IShellWorker> StartAsync(Stream? inputStream, bool disposeOnCompletion, CancellationToken cancellationToken = default) {
            IShellWorker worker = Shell.CreateShellWorker(this, inputStream, disposeOnCompletion);
            await worker.StartAsync(cancellationToken);
            return worker;
        }

        /// <summary>
        /// Returns the evaluated output of the shell script as a string.
        /// Executes the command synchronously and returns the result.
        /// </summary>
        /// <returns>The command output as a string</returns>
        public override string ToString() {
            return Evaluate();
        }
    }
}
