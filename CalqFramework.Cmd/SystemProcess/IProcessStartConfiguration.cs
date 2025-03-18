namespace CalqFramework.Cmd.SystemProcess {
    public interface IProcessStartConfiguration {
        TextReader In { get; }
        TextWriter InWriter { get; }
        string WorkingDirectory { get; }
    }
}