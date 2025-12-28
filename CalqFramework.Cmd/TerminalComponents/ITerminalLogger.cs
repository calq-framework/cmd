namespace CalqFramework.Cmd.TerminalComponents {

    public interface ITerminalLogger {

        /// <summary>
        /// Logs a shell script before it is executed in RUN operations.
        /// Called only for interactive command execution, not for CMD operations that return string results.
        /// </summary>
        public void Log(ShellScript shellScript);
    }
}
