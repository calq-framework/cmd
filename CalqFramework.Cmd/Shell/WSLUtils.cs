using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Text;

namespace CalqFramework.Cmd.Shell {
#pragma warning disable CA1416 // Validate platform compatibility

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

        internal static string WslToWindowsPath(string wslPath) {
            if (string.IsNullOrWhiteSpace(wslPath)) {
                throw new ArgumentException("WSL path cannot be null, empty, or whitespace.", nameof(wslPath));
            }

            if (wslPath.StartsWith("/mnt/", StringComparison.Ordinal)) {
                // Split "/mnt/c/foo/bar" → ["c","foo","bar"]
                var parts = wslPath
                    .Substring("/mnt/".Length)
                    .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 0) {
                    throw new ArgumentException($"Invalid WSL mount syntax: '{wslPath}'", nameof(wslPath));
                }

                var drivePart = parts[0];
                if (drivePart.Length != 1 || !char.IsLetter(drivePart[0])) {
                    throw new ArgumentException($"Invalid drive letter '{drivePart}' in WSL path.", nameof(wslPath));
                }

                // Upper-case the drive letter for Windows style
                var drive = char.ToUpperInvariant(drivePart[0]) + ":";

                // Build the full Windows path
                var tailSegments = parts.Skip(1).ToArray();

                if (tailSegments.Length == 0) {
                    // e.g. "/mnt/c" → "C:\"
                    return drive + Path.DirectorySeparatorChar;
                } else {
                    // ["C:", "Users", "foo"] → "C:\Users\foo"
                    return Path.Combine(new[] { drive }.Concat(tailSegments).ToArray());
                }
            } else {
                // Non-/mnt paths go via the UNC \\wsl$\ distribution share
                var distro = GetDefaultWslDistributionName();
                if (distro == null) {
                    throw new ArgumentException("Could not determine default WSL distribution name.", nameof(wslPath));
                }

                var segments = wslPath
                    .TrimStart('/')
                    .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                var root = $@"\\wsl$\{distro}";

                if (segments.Length == 0) {
                    // e.g. "/" → "\\wsl$\Ubuntu\"
                    return root + Path.DirectorySeparatorChar;
                } else {
                    // ["\\wsl$\Ubuntu", "home", "user"] → "\\wsl$\Ubuntu\home\user"
                    return Path.Combine(new[] { root }.Concat(segments).ToArray());
                }
            }
        }

        private static string? GetDefaultWslDistributionName() {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Lxss")) {
                if (key == null) {
                    return null;
                }

                var defaultGuid = key.GetValue("DefaultDistribution") as string;
                if (string.IsNullOrEmpty(defaultGuid)) {
                    return null;
                }

                using (var distroKey = key.OpenSubKey(defaultGuid)) {
                    if (distroKey == null) {
                        return null;
                    }

                    return distroKey.GetValue("DistributionName") as string;
                }
            }
        }

        [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
        private static extern int WNetGetConnection(string localName, StringBuilder remoteName, ref int length);
    }

#pragma warning restore CA1416 // Validate platform compatibility
}