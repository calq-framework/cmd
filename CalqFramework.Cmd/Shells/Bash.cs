using CalqFramework.Cmd.Execution;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace CalqFramework.Cmd.Shells;
public class Bash : ShellBase {
    internal bool ExpectWSL { get; init; } = true;

    public override string GetInternalPath(string hostPath) {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT && ExpectWSL) {
            return WindowsToWslPath(hostPath);
        }
        return hostPath;
    }

    internal override ProcessExecutionInfo GetProcessExecutionInfo(string workingDirectory, string script) {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT && ExpectWSL) {
            script = $"cd {WindowsToWslPath(workingDirectory)}\n" + script;
        }

        script = script.Replace("\r\n", "\n");

        string pattern = @"(?<!\\)\n"; // match lines not preceded by '\'
        string[] commands = Regex.Split(script, pattern);

        var trappedScript = new StringBuilder();
        trappedScript.Append("set -e\n");
        var i = 0;
        foreach (string cmd in commands) {
            trappedScript.Append($"trap '>&2 echo \"\nExited with code $? at line {i}.\"' exit\n");
            // TODO append if exit not 0 then exit; also remove set -e
            trappedScript.Append($"{cmd}\n");
            i += RegexGenerator.Newline.Unix().Matches(cmd).Count + 1;
        }
        trappedScript.Append($"trap '' exit\n");

        var scriptBytes = Encoding.UTF8.GetBytes(script);
        var scriptBase64 = Convert.ToBase64String(scriptBytes);
        var evalCommand = $"eval \"\"$(echo \"{scriptBase64}\" | base64 -d)\"\"";

        //Arguments = $"-c 'script -E never -e -q -f /dev/null -c \"{evalCommand}\"'",

        return new ProcessExecutionInfo("bash", $"-c \"{evalCommand}\"");
    }

    private static string? GetUncPathFromDrive(string driveLetter) {
        var maxPathSize = 256;
        var sb = new StringBuilder(maxPathSize);
        var result = WNetGetConnection(driveLetter, sb, ref maxPathSize);

        if (result == 0) {
            return sb.ToString();
        }

        return null;
    }

    private static string WindowsToWslPath(string windowsPath) {
        const string wslPrefix = @"\\wsl$\";

        if (windowsPath.StartsWith(wslPrefix, StringComparison.OrdinalIgnoreCase)) {
            var remainder = windowsPath.Substring(wslPrefix.Length);
            var index = remainder.IndexOf('\\');

            if (index >= 0) {
                return remainder.Substring(index).Replace('\\', '/');
            }

            return "/";
        }

        if (windowsPath.Length >= 2 && windowsPath[1] == ':') {
            var drive = windowsPath.Substring(0, 2);
            var uncPath = GetUncPathFromDrive(drive);

            if (uncPath != null && uncPath.StartsWith(wslPrefix, StringComparison.OrdinalIgnoreCase)) {
                var relativePath = windowsPath;
                return WindowsToWslPath(uncPath + relativePath);
            } else {
                var driveLetter = char.ToLower(windowsPath[0]);
                var pathWithoutDrive = windowsPath.Substring(2).Replace('\\', '/');
                return $"/mnt/{driveLetter}{pathWithoutDrive}";
            }
        }

        throw new ArgumentException("Unsupported path format", nameof(windowsPath));
    }
    [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
    private static extern int WNetGetConnection(string localName, StringBuilder remoteName, ref int length);
}
