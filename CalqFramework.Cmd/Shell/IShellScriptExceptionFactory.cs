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
        /// <returns>A formatted ShellScriptException with enriched error information</returns>
        Task<ShellScriptException> CreateAsync(ShellScript shellScript, IShellWorker shellWorker, ShellWorkerException exception, string? output, CancellationToken cancellationToken = default);
    }
}
