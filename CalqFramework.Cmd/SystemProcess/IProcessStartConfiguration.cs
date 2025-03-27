using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd.SystemProcess {
    public interface IProcessStartConfiguration {
        IShellWorkerErrorHandler ErrorHandler { get; }
        TextReader In { get; }
        TextWriter InWriter { get; }
        string WorkingDirectory { get; }
    }
}