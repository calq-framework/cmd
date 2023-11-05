using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace CalqFramework.Terminal.RegexGenerator {
    partial class Newline {
        [GeneratedRegex(@"\n")]
        public static partial Regex Unix();

        [GeneratedRegex(@"\r\n")]
        public static partial Regex DOS();
    }
}
