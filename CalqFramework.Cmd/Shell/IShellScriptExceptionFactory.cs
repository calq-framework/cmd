namespace CalqFramework.Cmd.Shell {

    /// <summary>
    /// Factory interface for creating ShellScriptException instances.
    /// Handles conversion from ShellWorkerException to user-facing exceptions.
    /// </summary>

    public interface IShellScriptExceptionFactory {

        /// <summary>
        /// Creates user-facing ShellScriptException instances from low-level ShellWorkerException errors.
        /// Enriches exceptions with context like command text, error output, and execution details.
        /// Enables consistent error handling across different shell implementations.
        /// </summary>
        /// <param name="shellScript">The shell script that failed during execution</param>
        /// <param name="shellWorker">The worker that encountered the error</param>
        /// <param name="exception">The underlying worker exception that occurred</param>
        /// <param name="output">Any output captured before the error occurred</param>
        /// <param name="cancellationToken">Token to cancel the exception creation</param>
        /// <returns>A formatted ShellScriptException with enriched error information</returns>
        Task<ShellScriptException> CreateAsync(ShellScript shellScript, IShellWorker shellWorker, ShellWorkerException exception, string? output, CancellationToken cancellationToken = default);
    }
}
