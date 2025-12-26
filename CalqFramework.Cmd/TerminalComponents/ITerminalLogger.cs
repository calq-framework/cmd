namespace CalqFramework.Cmd.TerminalComponents {

    public interface ITerminalLogger {

        /// <summary>
        /// Logs the execution of a shell script to the output stream.
        /// Displays the command being executed for debugging purposes.
        /// </summary>
        /// <param name="shellScript">The shell script to log information about</param>
        public void Log(ShellScript shellScript);
    }
}
