namespace CalqFramework.Cmd.Shell {

    /// <summary>
    /// Interface for processing shell command output.
    /// Default implementation trims the last newline from CMD results.
    /// </summary>

    public interface IShellScriptPostprocessor {

        /// <summary>
        /// Processes and cleans up command output before returning to the caller.
        /// Default implementation removes trailing newlines from CMD command results.
        /// Can be customized to handle shell-specific output formatting requirements.
        /// </summary>
        /// <returns>Processed output ready for consumption</returns>
        string ProcessOutput(string output);
    }
}
