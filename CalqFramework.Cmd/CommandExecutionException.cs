namespace CalqFramework.Cmd;
public class CommandExecutionException : Exception {
    public int ExitCode { get; }

    public CommandExecutionException(int exitCode, string? message) : this(exitCode, message, null) {
    }

    public CommandExecutionException(int exitCode, string? message, Exception? innerException) : base(message, innerException) {
        ExitCode = exitCode;
    }
}
