namespace CalqFramework.Cmd.Shell {

    /// <summary>
    /// Core interface for all shell implementations. Defines contract for
    /// worker creation, path mapping, and stream/exception handling.
    /// Implemented by CommandLine, Bash, PythonTool, HttpTool, and ShellTool.
    /// </summary>

    public interface IShell {
        /// <summary>
        /// Factory for creating enriched exceptions when shell commands fail.
        /// Converts low-level worker errors into user-friendly ShellScriptException instances.
        /// </summary>
        IShellScriptExceptionFactory ExceptionFactory { get; }
        
        /// <summary>
        /// Default input stream for shell commands when no explicit input is provided.
        /// Can be null if no default input is configured for this shell.
        /// </summary>
        Stream? In { get; }
        
        /// <summary>
        /// Processor for cleaning up and formatting command output.
        /// Handles shell-specific output formatting like removing trailing newlines.
        /// </summary>
        IShellScriptPostprocessor Postprocessor { get; }

        /// <summary>
        /// Creates a worker to execute the specified shell script.
        /// Worker manages the command lifecycle and provides access to output streams.
        /// </summary>
        /// <param name="shellScript">The script to execute</param>
        /// <param name="disposeOnCompletion">Whether to auto-dispose the worker when output reading completes</param>
        /// <returns>A worker instance ready to execute the command</returns>
        IShellWorker CreateShellWorker(ShellScript shellScript, bool disposeOnCompletion = true);

        /// <summary>
        /// Creates a worker to execute the specified shell script with custom input.
        /// Allows providing input data to be fed to the command's stdin.
        /// </summary>
        /// <param name="shellScript">The script to execute</param>
        /// <param name="inputStream">Input stream to pipe to the command's stdin</param>
        /// <param name="disposeOnCompletion">Whether to auto-dispose the worker when output reading completes</param>
        /// <returns>A worker instance ready to execute the command with the provided input</returns>
        IShellWorker CreateShellWorker(ShellScript shellScript, Stream? inputStream, bool disposeOnCompletion = true);

        /// <summary>
        /// Maps an internal path representation to the host system's path format.
        /// Handles path translation for containerized or virtualized shell environments.
        /// </summary>
        /// <param name="internalPath">Path in the shell's internal format</param>
        /// <returns>Path formatted for the host system</returns>
        string MapToHostPath(string internalPath);

        /// <summary>
        /// Maps a host system path to the shell's internal path representation.
        /// Enables path translation when working with different shell environments.
        /// </summary>
        /// <param name="hostPath">Path in the host system's format</param>
        /// <returns>Path formatted for the shell's internal use</returns>
        string MapToInternalPath(string hostPath);
    }
}
