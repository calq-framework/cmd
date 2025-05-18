namespace CalqFramework.Cmd.Shell {

    public interface IShellScriptExceptionFactory {

        Task<ShellScriptException> CreateAsync(ShellScript shellScript, IShellWorker shellWorker, ShellWorkerException exception, string? output, CancellationToken cancellationToken = default);
    }
}
