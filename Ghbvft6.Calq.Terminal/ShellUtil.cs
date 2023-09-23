using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ghbvft6.Calq.Terminal;
public class ShellUtil
{
    private static readonly Shell shell;

    static ShellUtil()
    {
        switch (Environment.OSVersion.Platform)
        {
            case PlatformID.Win32NT:
                shell = new CommandLine();
                break;
            case PlatformID.Unix:
                shell = new Bash();
                break;
            default:
                shell = new CommandLine();
                break;
        }
    }

    public static string CMD(string script)
    {
        return shell.CMD(script);
    }
}
