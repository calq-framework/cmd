namespace CalqFramework.Cmd.Shell {
    internal class ShellCommandStartConfiguration : IShellCommandStartConfiguration {
        public ShellCommandStartConfiguration() {
            In = Console.In;
            InInterceptor = Console.Out;
            WorkingDirectory = Environment.CurrentDirectory;
        }

        public ShellCommandStartConfiguration(IShellCommandStartConfiguration shellCommandStartConfiguration) {
            In = shellCommandStartConfiguration.In;
            InInterceptor = shellCommandStartConfiguration.InInterceptor;
            WorkingDirectory = shellCommandStartConfiguration.WorkingDirectory;
        }

        public TextReader In { get; init; }
        public TextWriter InInterceptor { get; init; }
        public string WorkingDirectory { get; init; }
    }
}
