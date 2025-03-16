using CalqFramework.Cmd.SystemProcess;
using System.Runtime.InteropServices;
using System.Text;

namespace CalqFramework.Cmd.Shells;

public abstract class ShellBase : IShell {

    static ShellBase() {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
            var output = new StringWriter();
            var pr = new ProcessRunner();
            pr.Run(new ProcessRunInfo("bash", "-c \"uname -s\""), new ProcessRunConfiguration() { In = TextReader.Null, Out = output }).ConfigureAwait(false).GetAwaiter().GetResult();
            IsRunningBashOnWSL = output.ToString().TrimEnd() switch {
                "Linux" => true,
                "Darwin" => true,
                _ => false
            };
        } else {
            IsRunningBashOnWSL = false;
        }
    }

    internal static bool IsRunningBashOnWSL { get; }

    internal abstract bool IsUsingWSL { get; }

    public void Execute(string script, IProcessRunConfiguration processRunConfiguration, CancellationToken cancellationToken = default) {
        ExecuteAsync(script, processRunConfiguration, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public async Task ExecuteAsync(string script, IProcessRunConfiguration processRunConfiguration, CancellationToken cancellationToken = default) {
        string AddLineNumbers(string input) {
            var i = 0;
            return string.Join('\n', input.Split('\n').Select(x => $"{i++}: {x}"));
        }

        var processRunInfo = GetProcessRunInfo(processRunConfiguration.WorkingDirectory, script);
        using var processRunner = new ProcessRunner();

        try {
            await processRunner.Run(processRunInfo, processRunConfiguration, cancellationToken);
        } catch (ProcessExecutionException ex) {
            throw new CommandExecutionException(ex.ExitCode, $"\n{AddLineNumbers(script)}\n\nExit code:\n{ex.ExitCode}\n\nError:\n{ex.Message}", ex); // TODO formalize error handling
        }
    }

    public string GetInternalPath(string hostPath) {
        if (IsUsingWSL) {
            return WindowsToWslPath(hostPath);
        }
        return hostPath;
    }

    internal static string WindowsToWslPath(string windowsPath) {
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

    internal abstract ProcessRunInfo GetProcessRunInfo(string workingDirectory, string script);

    private static string? GetUncPathFromDrive(string driveLetter) {
        var maxPathSize = 256;
        var sb = new StringBuilder(maxPathSize);
        var result = WNetGetConnection(driveLetter, sb, ref maxPathSize);

        if (result == 0) {
            return sb.ToString();
        }

        return null;
    }
    [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
    private static extern int WNetGetConnection(string localName, StringBuilder remoteName, ref int length);
}
