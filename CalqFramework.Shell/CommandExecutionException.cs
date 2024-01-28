using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalqFramework.Shell;
public class CommandExecutionException : Exception
{
    public int ExitCode { get; }

    public CommandExecutionException(string message, int exitCode) : base(message)
    {
        ExitCode = exitCode;
    }
}
