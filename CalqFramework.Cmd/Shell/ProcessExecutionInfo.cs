namespace CalqFramework.Cmd.Shell {

    /// <summary>
    /// Contains process execution information including the executable path and command arguments.
    /// Used internally by process workers to configure process startup.
    /// </summary>
    public class ProcessExecutionInfo(string fileName, string arguments) {
        /// <summary>
        /// Command line arguments to pass to the process.
        /// </summary>
        public string Arguments { get; } = arguments;
        
        /// <summary>
        /// Path to the executable file to run.
        /// </summary>
        public string FileName { get; } = fileName;
    }
}
