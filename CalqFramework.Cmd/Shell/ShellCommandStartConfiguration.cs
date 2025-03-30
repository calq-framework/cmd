namespace CalqFramework.Cmd.Shell {
    internal class ShellCommandStartConfiguration : IShellCommandStartConfiguration {
        public ShellCommandStartConfiguration() {
            In = Console.In;
            InWriter = Console.Out;
            WorkingDirectory = Environment.CurrentDirectory;
        }

        public ShellCommandStartConfiguration(IShellCommandStartConfiguration shellCommandStartConfiguration) {
            In = shellCommandStartConfiguration.In;
            InWriter = shellCommandStartConfiguration.InWriter;
            WorkingDirectory = shellCommandStartConfiguration.WorkingDirectory;
        }

        public TextReader In { get; init; }
        public TextWriter InWriter { get; init; }
        public string WorkingDirectory { get; init; }
    }
}
