namespace CalqFramework.Cmd.SystemProcess {
    internal class ProcessRunInfo {
        public ProcessRunInfo(string fileName, string arguments) {
            FileName = fileName;
            Arguments = arguments;
        }

        public string Arguments { get; }
        public string FileName { get; }
    }
}
