namespace CalqFramework.Cmd.Shell {
    public class ShellScriptExceptionFactory : IShellScriptExceptionFactory {
        public async Task<ShellScriptException> CreateAsync(ShellScript shellScript, IShellWorker shellWorker, ShellWorkerException exception, string? output, CancellationToken cancellationToken = default) {
            var errorMessage = await shellWorker.ReadErrorMessageAsync();
            var formattedErrorMessage = string.IsNullOrEmpty(errorMessage) == false ? "\n\nError:\n" + errorMessage : "\n\nError:\n" + output ?? "";

            var err = new ShellScriptException(exception.ErrorCode, $"\n{shellScript.Script}\n\n{exception.Message}{formattedErrorMessage}", exception);
            return err;
        }
    }
}
