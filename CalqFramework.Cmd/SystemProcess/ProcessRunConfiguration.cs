namespace CalqFramework.Cmd.SystemProcess {
    internal class ProcessRunConfiguration : IProcessRunConfiguration {
        public ProcessRunConfiguration() {
            ErrorHandler = new ProcessErrorHandler();
            In = Console.In;
            Out = Console.Out;
            WorkingDirectory = Environment.CurrentDirectory;
        }

        public ProcessRunConfiguration(IProcessRunConfiguration processRunConfiguration) {
            ErrorHandler = processRunConfiguration.ErrorHandler;
            In = processRunConfiguration.In;
            Out = processRunConfiguration.Out;
            WorkingDirectory = processRunConfiguration.WorkingDirectory;
        }

        public ProcessErrorHandler ErrorHandler { get; init; }
        public TextReader In { get; init; }
        public TextWriter Out { get; init; }
        public string WorkingDirectory { get; init; }
    }
}
