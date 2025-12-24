using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace CalqFramework.Cmd.Shell {
#pragma warning disable CA1416 // Validate platform compatibility

    /// <summary>
    /// Utilities for Windows Subsystem for Linux (WSL) path conversion.
    /// Handles mapping between Windows paths (C:\Users) and WSL paths (/mnt/c/Users).
    /// Supports both WSL and network drive scenarios.
    /// </summary>

    internal static class WSLUtils {

        /// <summary>
        /// Converts Windows path to WSL path format.
        /// Examples: C:\Users → /mnt/c/Users, \\wsl$\Ubuntu\home → /home
        /// </summary>
        internal static string WindowsToWslPath(string windowsPath) {
            const string wslPrefix = @"\\wsl$\";

            if (windowsPath.StartsWith(wslPrefix, StringComparison.OrdinalIgnoreCase)) {
                string remainder = windowsPath.Substring(wslPrefix.Length);
                int index = remainder.IndexOf('\\');

                if (index >= 0) {
                    return remainder.Substring(index).Replace('\\', '/');
                }

                return "/";
            }

            if (windowsPath.Length >= 2 && windowsPath[1] == ':') {
                string drive = windowsPath.Substring(0, 2);
                string? uncPath = GetUncPathFromDrive(drive);

                if (uncPath != null && uncPath.StartsWith(wslPrefix, StringComparison.OrdinalIgnoreCase)) {
                    string relativePath = windowsPath;
                    return WindowsToWslPath(uncPath + relativePath);
                } else {
                    char driveLetter = char.ToLower(windowsPath[0]);
                    string pathWithoutDrive = windowsPath.Substring(2).Replace('\\', '/');
                    return $"/mnt/{driveLetter}{pathWithoutDrive}";
                }
            }

            throw new ArgumentException("Unsupported path format", nameof(windowsPath));
        }

        private static string? GetUncPathFromDrive(string driveLetter) {
            int maxPathSize = 256;
            var sb = new StringBuilder(maxPathSize);
            int result = WNetGetConnection(driveLetter, sb, ref maxPathSize);

            if (result == 0) {
                return sb.ToString();
            }

            return null;
        }

        /// <summary>
        /// Converts WSL path to Windows path format.
        /// Examples: /mnt/c/Users → C:\Users, /home/user → \\wsl$\distro\home\user
        /// </summary>
        internal static string WslToWindowsPath(string wslPath) {
            if (string.IsNullOrWhiteSpace(wslPath)) {
                throw new ArgumentException("WSL path cannot be null, empty, or whitespace.", nameof(wslPath));
            }

            if (wslPath.StartsWith("/mnt/", StringComparison.Ordinal)) {
                // Split "/mnt/c/foo/bar" → ["c","foo","bar"]
                string[] parts = wslPath
                    .Substring("/mnt/".Length)
                    .Split(['/'], StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 0) {
                    throw new ArgumentException($"Invalid WSL mount syntax: '{wslPath}'", nameof(wslPath));
                }

                string drivePart = parts[0];
                if (drivePart.Length != 1 || !char.IsLetter(drivePart[0])) {
                    throw new ArgumentException($"Invalid drive letter '{drivePart}' in WSL path.", nameof(wslPath));
                }

                // Upper-case the drive letter for Windows style
                string drive = char.ToUpperInvariant(drivePart[0]) + ":";

                // Build the full Windows path
                string[] tailSegments = [.. parts.Skip(1)];

                if (tailSegments.Length == 0) {
                    // e.g. "/mnt/c" → "C:\"
                    return drive + Path.DirectorySeparatorChar;
                } else {
                    // ["C:", "Users", "foo"] → "C:\Users\foo"
                    return Path.Combine([drive, .. tailSegments]);
                }
            } else {
                // Non-/mnt paths go via the UNC \\wsl$\ distribution share
                string? distro = GetDefaultWslDistributionName() ?? throw new ArgumentException("Could not determine default WSL distribution name.", nameof(wslPath));
                string[] segments = wslPath
                    .TrimStart('/')
                    .Split(['/'], StringSplitOptions.RemoveEmptyEntries);

                string root = $@"\\wsl$\{distro}";

                if (segments.Length == 0) {
                    // e.g. "/" → "\\wsl$\Ubuntu\"
                    return root + Path.DirectorySeparatorChar;
                } else {
                    // ["\\wsl$\Ubuntu", "home", "user"] → "\\wsl$\Ubuntu\home\user"
                    return Path.Combine([root, .. segments]);
                }
            }
        }

        private static string? GetDefaultWslDistributionName() {
            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Lxss")) {
                if (key == null) {
                    return null;
                }

                string? defaultGuid = key.GetValue("DefaultDistribution") as string;
                if (string.IsNullOrEmpty(defaultGuid)) {
                    return null;
                }

                using (RegistryKey? distroKey = key.OpenSubKey(defaultGuid)) {
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
