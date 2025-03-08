namespace CalqFramework.Cmd {
    internal class ScriptExecutionInfo {
        public ScriptExecutionInfo(string fileName, string arguments) {
            FileName = fileName;
            Arguments = arguments;
        }

        public string Arguments { get; }
        public string FileName { get; }
    }
}
