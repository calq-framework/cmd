namespace CalqFramework.Cmd.SystemProcess {
    public interface IProcessRunConfiguration : IProcessStartConfiguration {
        IProcessErrorHandler ErrorHandler { get; }
        TextWriter Out { get; }
    }
}