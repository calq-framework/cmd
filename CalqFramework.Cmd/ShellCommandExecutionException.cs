namespace CalqFramework.Cmd;
public class ShellCommandExecutionException : Exception {
    public int ExitCode { get; }

    public ShellCommandExecutionException(int exitCode, string? message) : this(exitCode, message, null) {
    }

    public ShellCommandExecutionException(int exitCode, string? message, Exception? innerException) : base(message, innerException) {
        ExitCode = exitCode;
    }
}
