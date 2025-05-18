namespace CalqFramework.Cmd.Shell {

    public class ProcessExecutionInfo(string fileName, string arguments) {
        public string Arguments { get; } = arguments;
        public string FileName { get; } = fileName;
    }
}
