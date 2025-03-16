namespace CalqFramework.Cmd.SystemProcess {
    public interface IProcessRunConfiguration {
        IProcessErrorHandler ErrorHandler { get; }
        TextReader In { get; }
        TextWriter Out { get; }
        string WorkingDirectory { get; }
    }
}