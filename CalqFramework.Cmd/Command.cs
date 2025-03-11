using CalqFramework.Cmd.Shells;

namespace CalqFramework.Cmd {
    public class Command {
        public Command(IShell shell, string workingDirectory, string script, TextReader inReader) {
            In = inReader;
            Shell = shell;
            WorkingDirectory = workingDirectory;
            Script = script;
            Out = new StringWriter();
        }

        private TextReader In { get; set; }
        private StringWriter Out { get; }
        private string Script { get; }
        private IShell Shell { get; }
        private string WorkingDirectory { get; }

        public static implicit operator string(Command obj) {
            return obj.Run();
        }

        public static Command operator |(Command a, Command b) {
            b.In = new StringReader(a.Run()); // TODO relay stream in loop instead of converting to string (fire a task and forget)
            return b;
        }

        string Run() {
            Shell.ExecuteAsync(WorkingDirectory, Script, In, Out).ConfigureAwait(false).GetAwaiter().GetResult();
            return Out.ToString();
        }
    }
}