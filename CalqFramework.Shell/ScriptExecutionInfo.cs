namespace CalqFramework.Shell {
    internal class ScriptExecutionInfo {
        public ScriptExecutionInfo(string fileName, string arguments) {
            FileName = fileName;
            Arguments = arguments;
        }

        public string Arguments { get; }
        public string FileName { get; }
    }
}
