namespace CalqFramework.Cmd.SystemProcess {
    public interface IProcessRunConfiguration {
        TextReader In { get; }
        TextWriter Out { get; }
        string WorkingDirectory { get; }
        ProcessErrorHandler ErrorHandler { get; }
    }
}