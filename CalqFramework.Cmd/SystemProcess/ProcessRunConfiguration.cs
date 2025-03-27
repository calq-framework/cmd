using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd.SystemProcess {
    internal class ProcessRunConfiguration : IProcessRunConfiguration {
        public ProcessRunConfiguration() {
            ErrorHandler = new ShellWorkerErrorHandler();
            In = Console.In;
            InWriter = Console.Out;
            Out = Console.Out;
            WorkingDirectory = Environment.CurrentDirectory;
        }

        public ProcessRunConfiguration(IProcessRunConfiguration processRunConfiguration) {
            ErrorHandler = processRunConfiguration.ErrorHandler;
            In = processRunConfiguration.In;
            Out = processRunConfiguration.Out;
            WorkingDirectory = processRunConfiguration.WorkingDirectory;
        }

        public IShellWorkerErrorHandler ErrorHandler { get; init; }
        public TextReader In { get; init; }
        public TextWriter InWriter { get; init; }
        public TextWriter Out { get; init; }
        public string WorkingDirectory { get; init; }
    }
}
