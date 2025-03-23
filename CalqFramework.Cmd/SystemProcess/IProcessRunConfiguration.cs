namespace CalqFramework.Cmd.SystemProcess {
    public interface IProcessRunConfiguration : IProcessStartConfiguration {
        TextWriter Out { get; }
    }
}