using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd.Shells;
using System.Diagnostics;
using System.Reflection;

namespace CalqFramework.Cmd;

/// <summary>
/// Factory for creating underlying shells used by LocalTool
/// </summary>
public class LocalToolFactory : ILocalToolFactory
{
    internal static ILocalToolFactory Factory { get; set; } = new LocalToolFactory();

    public IShell CreateLocalTool()
    {
        return new ShellTool(new CommandLine(), GetCurrentExecutablePath());
    }

    private static string GetCurrentExecutablePath()
    {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var location = assembly.Location;
        
        if (string.IsNullOrEmpty(location))
        {
            var processPath = Process.GetCurrentProcess().MainModule?.FileName;
            if (!string.IsNullOrEmpty(processPath))
            {
                return processPath;
            }
            
            return "dotnet";
        }
        
        return location;
    }
}