using System.Runtime.InteropServices;
using System.Text;

namespace CalqFramework.Cmd.Shell {
    internal static class WSLUtils {
        internal static string WindowsToWslPath(string windowsPath) {
            const string wslPrefix = @"\\wsl$\";

            if (windowsPath.StartsWith(wslPrefix, StringComparison.OrdinalIgnoreCase)) {
                var remainder = windowsPath.Substring(wslPrefix.Length);
                var index = remainder.IndexOf('\\');

                if (index >= 0) {
                    return remainder.Substring(index).Replace('\\', '/');
                }

                return "/";
            }

            if (windowsPath.Length >= 2 && windowsPath[1] == ':') {
                var drive = windowsPath.Substring(0, 2);
                var uncPath = GetUncPathFromDrive(drive);

                if (uncPath != null && uncPath.StartsWith(wslPrefix, StringComparison.OrdinalIgnoreCase)) {
                    var relativePath = windowsPath;
                    return WindowsToWslPath(uncPath + relativePath);
                } else {
                    var driveLetter = char.ToLower(windowsPath[0]);
                    var pathWithoutDrive = windowsPath.Substring(2).Replace('\\', '/');
                    return $"/mnt/{driveLetter}{pathWithoutDrive}";
                }
            }

            throw new ArgumentException("Unsupported path format", nameof(windowsPath));
        }

        private static string? GetUncPathFromDrive(string driveLetter) {
            var maxPathSize = 256;
            var sb = new StringBuilder(maxPathSize);
            var result = WNetGetConnection(driveLetter, sb, ref maxPathSize);

            if (result == 0) {
                return sb.ToString();
            }

            return null;
        }

        [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
        private static extern int WNetGetConnection(string localName, StringBuilder remoteName, ref int length);
    }
}
