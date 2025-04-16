
namespace CalqFramework.Cmd.Shell {
    public interface IShellWorker : IDisposable {
        IShellWorker? PipedWorker { get; }
        ShellScript ShellScript { get; }
        Stream StandardOutput { get; }

        Task Start();

        Task WaitForSuccess(string? output = null);
    }
}