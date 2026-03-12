using System.Text;
using System.Text.RegularExpressions;
using CalqFramework.Cmd.Shells;

namespace CalqFramework.Cmd.Shell.SystemProcess;

/// <summary>
///     Process worker for Bash shell execution with WSL support.
///     Handles script encoding, error trapping, and WSL path conversion.
/// </summary>
public class BashWorker(ShellScript shellScript, Stream? inputStream, bool disposeOnCompletion = true)
    : ProcessWorkerBase(shellScript, inputStream, disposeOnCompletion) {
    internal static bool IsUsingWSL => Bash.IsRunningBashOnWSL;

    internal override ProcessExecutionInfo GetProcessExecutionInfo(string workingDirectory, string script) {
        if (IsUsingWSL) {
            script = $"cd {WSLUtils.WindowsToWslPath(workingDirectory)}\n" + script;
        }

        script = script.Replace("\r\n", "\n");

        string pattern = @"(?<!\\)\n"; // match lines not preceded by '\'
        string[] commands = Regex.Split(script, pattern);

        StringBuilder trappedScript = new();
        trappedScript.Append("set -e\n");
        int i = 0;
        foreach (string cmd in commands) {
            trappedScript.Append($"trap '>&2 echo \"\nExited with code $? at line {i}.\"' exit\n");
            // TODO append if exit not 0 then exit; also remove set -e
            trappedScript.Append($"{cmd}\n");
            i += cmd.Count(c => c == '\n') + 1;
        }

        trappedScript.Append("trap '' exit\n");

        byte[] scriptBytes = Encoding.UTF8.GetBytes(script); // FIXME line split regex pattern doesnt handle <<EOF
        string scriptBase64 = Convert.ToBase64String(scriptBytes);
        string evalCommand = $"eval \"\"$(echo \"{scriptBase64}\" | base64 -d)\"\"";

        //Arguments = $"-c 'script -E never -e -q -f /dev/null -c \"{evalCommand}\"'",

        return new ProcessExecutionInfo("bash", $"-c \"{evalCommand}\"");
    }
}
