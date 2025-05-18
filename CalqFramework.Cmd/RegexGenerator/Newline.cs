using System.Text.RegularExpressions;

namespace CalqFramework.Cmd.RegexGenerator {

    internal partial class Newline {

        [GeneratedRegex(@"\r\n")]
        public static partial Regex DOS();

        [GeneratedRegex(@"\n")]
        public static partial Regex Unix();
    }
}