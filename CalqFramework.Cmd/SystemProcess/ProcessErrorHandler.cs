using System.Diagnostics;

namespace CalqFramework.Cmd.SystemProcess {
    public class ProcessErrorHandler : IProcessErrorHandler {
        public void AssertSuccess(ProcessExecutionInfo processExecutionInfo, IProcessRunConfiguration processRunConfiguration, Process process, string error) {
            // stderr might contain diagnostics/info instead of error message so don't throw just because not empty
            if (process.ExitCode != 0) {
                if (string.IsNullOrEmpty(error) && processRunConfiguration.Out is StringWriter stringOutputWriter) {
                    error = stringOutputWriter.ToString();
                }
                throw new ProcessExecutionException(process.ExitCode, error);
            }
        }
    }
}
