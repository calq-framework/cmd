using System.Diagnostics;

namespace CalqFramework.Shell;
public class CommandLine : ShellBase
{
    protected override Process InitializeProcess(string script)
    {
        int spaceIndex = script.IndexOf(' ');
        var command = script.Substring(0, spaceIndex);
        var arguments = script.Substring(spaceIndex + 1);

        ProcessStartInfo psi = new ProcessStartInfo
        {
            WorkingDirectory = Environment.CurrentDirectory,
            FileName = command,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            Arguments = arguments,
        };

        var process = new Process { StartInfo = psi };
        process.Start();

        return process;
    }
}
