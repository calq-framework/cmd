namespace CalqFramework.Cmd.Shell {
    public interface IShellCommandStartConfiguration {
        TextReader In { get; }
        TextWriter InWriter { get; }
        string WorkingDirectory { get; }
    }
}