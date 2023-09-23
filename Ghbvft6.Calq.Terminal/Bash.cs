using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ghbvft6.Calq.Terminal;
internal class Bash : Shell
{
    protected override Process InitializeProcess(string script)
    {
        script = script.Replace("\r\n", "\n");

        string pattern = @"(?<!\\)\n"; // match lines not preceded by '\'
        string[] commands = Regex.Split(script, pattern);

        var trappedScript = new StringBuilder();
        trappedScript.Append("set -e\n");
        var i = 0;
        foreach (string cmd in commands)
        {
            trappedScript.Append($"trap '>&2 echo \"\nExited with code $? at line {i}.\"' exit\n");
            // TODO append if exit not 0 then exit; also remove set -e
            trappedScript.Append($"{cmd}\n");
            i += RegexGenerator.Newline.Unix().Matches(cmd).Count + 1;
        }
        trappedScript.Append($"trap '' exit\n");

        var scriptBytes = Encoding.UTF8.GetBytes(script);
        var scriptBase64 = Convert.ToBase64String(scriptBytes);
        var evalCommand = $"eval \"\"$(echo \"{scriptBase64}\" | base64 -d)\"\"";

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "bash",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            //Arguments = $"-c 'script -E never -e -q -f /dev/null -c \"{evalCommand}\"'",
            Arguments = $"-c \"{evalCommand}\"",
        };

        var process = new Process { StartInfo = psi };
        process.Start();

        return process;
    }
}
