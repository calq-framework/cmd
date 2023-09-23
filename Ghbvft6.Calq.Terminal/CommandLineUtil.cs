using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ghbvft6.Calq.Terminal;
public class CommandLineUtil
{
    private static readonly Shell shell = new CommandLine();
    public static string CMD(string script)
    {
        return shell.CMD(script);
    }
}
