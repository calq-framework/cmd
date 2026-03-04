using System.Diagnostics;
using System.Reflection;
using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd.Shells;

namespace CalqFramework.Cmd;

/// <summary>
///     Factory for creating underlying shells used by LocalTool
/// </summary>
public class LocalToolFactory : ILocalToolFactory {
    internal static ILocalToolFactory Factory { get; set; } = new LocalToolFactory();

    public IShell CreateLocalTool() => new ShellTool(new CommandLine(), GetCurrentExecutablePath());

    private static string GetCurrentExecutablePath() {
        Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        string location = assembly.Location;

        if (string.IsNullOrEmpty(location)) {
            string? processPath = Process.GetCurrentProcess().MainModule?.FileName;
            if (!string.IsNullOrEmpty(processPath)) {
                return processPath;
            }

            return "dotnet";
        }

        return location;
    }
}
