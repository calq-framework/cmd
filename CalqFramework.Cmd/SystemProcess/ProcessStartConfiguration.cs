using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd.SystemProcess {
    internal class ProcessStartConfiguration : IProcessStartConfiguration {
        public ProcessStartConfiguration() {
            ErrorHandler = new ShellWorkerErrorHandler();
            In = Console.In;
            InWriter = Console.Out;
            WorkingDirectory = Environment.CurrentDirectory;
        }

        public ProcessStartConfiguration(IProcessStartConfiguration processStartConfiguration) {
            ErrorHandler = processStartConfiguration.ErrorHandler;
            In = processStartConfiguration.In;
            WorkingDirectory = processStartConfiguration.WorkingDirectory;
        }

        public IShellWorkerErrorHandler ErrorHandler { get; init; }
        public TextReader In { get; init; }
        public TextWriter InWriter { get; init; }
        public string WorkingDirectory { get; init; }
    }
}
