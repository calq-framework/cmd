using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ghbvft6.Calq.Terminal;
internal class CommandLine : Shell
{
    protected override Process InitializeProcess(string script)
    {
        int spaceIndex = script.IndexOf(' ');
        var command = script.Substring(0, spaceIndex);
        var arguments = script.Substring(spaceIndex + 1);

        ProcessStartInfo psi = new ProcessStartInfo
        {
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
