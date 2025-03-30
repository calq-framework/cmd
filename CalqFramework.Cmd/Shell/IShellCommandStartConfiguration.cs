namespace CalqFramework.Cmd.Shell {
    public interface IShellCommandStartConfiguration {
        TextReader In { get; }
        TextWriter InInterceptor { get; }
        string WorkingDirectory { get; }
    }
}