namespace CalqFramework.Cmd.Shell {
    public interface IShellCommandStartConfiguration {
        IShellWorkerErrorHandler ErrorHandler { get; }
        TextReader In { get; }
        TextWriter InWriter { get; }
        string WorkingDirectory { get; }
    }
}