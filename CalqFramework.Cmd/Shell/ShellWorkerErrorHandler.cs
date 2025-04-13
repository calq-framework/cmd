namespace CalqFramework.Cmd.Shell {
    public class ShellWorkerErrorHandler : IShellWorkerErrorHandler {
        public void AssertSuccess(string script, int exitCode, string errorMessage, string? output) {
            string AddLineNumbers(string input) {
                var i = 0;
                return string.Join('\n', input.Split('\n').Select(x => $"{i++}: {x}"));
            }

            var error = string.IsNullOrEmpty(errorMessage) == false ? errorMessage : output ?? "";

            // stderr might contain diagnostics/info instead of error errorMessage so don't throw just because not empty
            if (exitCode != 0) {
                throw new ShellScriptExecutionException(exitCode, $"\n{AddLineNumbers(script)}\n\nExit code:\n{exitCode}\n\nError:\n{error}");
            }
        }
    }
}
