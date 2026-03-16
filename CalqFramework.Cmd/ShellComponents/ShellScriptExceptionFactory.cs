namespace CalqFramework.Cmd.ShellComponents;

public class ShellScriptExceptionFactory : IShellScriptExceptionFactory {
    public async Task<ShellScriptException> CreateAsync(ShellScript shellScript, IShellWorker shellWorker, ShellWorkerException exception, string? output, CancellationToken cancellationToken = default) {
        string errorMessage = await shellWorker.ReadErrorMessageAsync(cancellationToken);
        string formattedErrorMessage = !string.IsNullOrEmpty(errorMessage) ? "\n\nError:\n" + errorMessage : "\n\nError:\n" + output ?? "";

        ShellScriptException err = new(exception.ErrorCode, $"\n{shellScript.Script}\n\n{exception.Message}{formattedErrorMessage}", exception);
        return err;
    }
}
