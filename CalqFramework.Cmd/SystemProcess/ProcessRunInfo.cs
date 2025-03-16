namespace CalqFramework.Cmd.SystemProcess {
    public class ProcessRunInfo {
        public ProcessRunInfo(string fileName, string arguments) {
            FileName = fileName;
            Arguments = arguments;
        }

        public string Arguments { get; }
        public string FileName { get; }
    }
}
