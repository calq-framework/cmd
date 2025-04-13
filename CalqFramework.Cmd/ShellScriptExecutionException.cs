namespace CalqFramework.Cmd;
public class ShellScriptExecutionException : Exception {
    public int ExitCode { get; }

    public ShellScriptExecutionException(int exitCode, string? message) : this(exitCode, message, null) {
    }

    public ShellScriptExecutionException(int exitCode, string? message, Exception? innerException) : base(message, innerException) {
        ExitCode = exitCode;
    }
}
