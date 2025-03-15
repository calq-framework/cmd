namespace CalqFramework.Cmd.SystemProcess {
    [Serializable]
    internal class ProcessExecutionException : Exception {
        public int ExitCode { get; }

        public ProcessExecutionException(int exitCode, string? message) : base(message) {
            ExitCode = exitCode;
        }
    }
}