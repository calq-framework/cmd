namespace CalqFramework.Cmd.Shell {

    /// <summary>
    /// Interface for shell workers that execute commands and provide stream access.
    /// Supports piping, error handling, and asynchronous execution.
    /// Used by both process-based and HTTP-based execution.
    /// </summary>

    public interface IShellWorker : IDisposable {
        /// <summary>
        /// Indicates whether this worker should be automatically disposed when output reading completes.
        /// Used to manage resource cleanup in pipeline scenarios.
        /// </summary>
        bool DisposeOnCompletion { get; }
        
        /// <summary>
        /// Reference to the previous worker in a command pipeline.
        /// Enables chaining commands with the | operator for parallel execution.
        /// </summary>
        IShellWorker? PipedWorker { get; }
        
        /// <summary>
        /// The shell script being executed by this worker.
        /// Contains the command text, working directory, and shell configuration.
        /// </summary>
        ShellScript ShellScript { get; }
        
        /// <summary>
        /// Stream providing access to the command's standard output.
        /// Supports both synchronous and asynchronous reading with error handling.
        /// </summary>
        ShellWorkerOutputStream StandardOutput { get; }

        /// <summary>
        /// Ensures all piped commands in the pipeline have completed execution.
        /// Waits for the entire command chain to finish before returning.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the wait operation</param>
        /// <returns>Task that completes when the entire pipeline is finished</returns>
        Task EnsurePipeIsCompletedAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Reads error messages from the command's standard error stream.
        /// Used for diagnostics and exception creation when commands fail.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the read operation</param>
        /// <returns>Error message text from stderr</returns>
        Task<string> ReadErrorMessageAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Starts the shell command execution asynchronously.
        /// Initializes the process or HTTP request and begins command execution.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the start operation</param>
        /// <returns>Task that completes when the command has started</returns>
        Task StartAsync(CancellationToken cancellationToken = default);
    }
}
