namespace CalqFramework.Cmd.Execution {
    internal class ProcessExecutionInfo {
        public ProcessExecutionInfo(string fileName, string arguments) {
            FileName = fileName;
            Arguments = arguments;
        }

        public string Arguments { get; }
        public string FileName { get; }
    }
}
