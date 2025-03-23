namespace CalqFramework.Cmd.SystemProcess {
    public interface IProcessStartConfiguration {
        IProcessErrorHandler ErrorHandler { get; }
        TextReader In { get; }
        TextWriter InWriter { get; }
        string WorkingDirectory { get; }
    }
}