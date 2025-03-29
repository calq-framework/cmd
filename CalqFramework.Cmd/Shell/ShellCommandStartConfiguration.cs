namespace CalqFramework.Cmd.Shell {
    internal class ShellCommandStartConfiguration : IShellCommandStartConfiguration {
        public ShellCommandStartConfiguration() {
            ErrorHandler = new ShellWorkerErrorHandler();
            In = Console.In;
            InWriter = Console.Out;
            WorkingDirectory = Environment.CurrentDirectory;
        }

        public ShellCommandStartConfiguration(IShellCommandStartConfiguration shellCommandStartConfiguration) {
            ErrorHandler = shellCommandStartConfiguration.ErrorHandler;
            In = shellCommandStartConfiguration.In;
            InWriter = shellCommandStartConfiguration.InWriter;
            WorkingDirectory = shellCommandStartConfiguration.WorkingDirectory;
        }

        public IShellWorkerErrorHandler ErrorHandler { get; init; }
        public TextReader In { get; init; }
        public TextWriter InWriter { get; init; }
        public string WorkingDirectory { get; init; }
    }
}
