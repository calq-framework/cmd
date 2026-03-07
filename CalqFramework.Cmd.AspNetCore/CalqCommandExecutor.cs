using System.Collections.Generic;
using System.IO;
using System.Linq;
using CalqFramework.Cli;
using CalqFramework.Cli.DataAccess;
using CalqFramework.Cli.Formatting;

namespace CalqFramework.Cmd.AspNetCore;

/// <summary>
///     Default command executor implementation using CalqFramework.Cli for command-line style parsing.
///     Uses AsIsClassMemberStringifier to preserve original method and parameter names without transformation.
/// </summary>
public class CalqCommandExecutor : ICalqCommandExecutor {
    /// <summary>
    ///     Executes a command using CLI-style argument parsing via CalqFramework.Cli.
    /// </summary>
    /// <param name="target">The target object containing the methods to execute.</param>
    /// <param name="args">Command-line arguments (e.g., ["MethodName", "--param", "value"]).</param>
    /// <returns>The result of the command execution.</returns>
    public object? Execute(object target, string[] args) => Execute(target, args, null);

    /// <summary>
    ///     Executes a command using CLI-style argument parsing via CalqFramework.Cli with output capture.
    /// </summary>
    /// <param name="target">The target object containing the methods to execute.</param>
    /// <param name="args">Command-line arguments (e.g., ["MethodName", "--param", "value"]).</param>
    /// <param name="output">Optional TextWriter to capture output. If null, uses Console.Out.</param>
    /// <returns>The result of the command execution.</returns>
    public object? Execute(object target, string[] args, TextWriter? output) {
        CommandLineInterface cli = new() {
            Out = output ?? Console.Out,
            CliComponentStoreFactory = new CliComponentStoreFactory {
                ClassMemberStringifier = new AsIsClassMemberStringifier()
            }
        };
        return cli.Execute(target, args);
    }

    /// <summary>
    ///     Returns class member names as-is without any transformation.
    ///     Preserves original naming conventions (e.g., PascalCase, snake_case).
    /// </summary>
    private class AsIsClassMemberStringifier : ClassMemberStringifierBase {
        protected override IEnumerable<string> GetAlternativeNames(string name,
            IEnumerable<CliNameAttribute> cliNameAttributes) {
            // Return empty list - no alternative names
            return [];
        }

        protected override IEnumerable<string> GetRequiredNames(string name,
            IEnumerable<CliNameAttribute> cliNameAttributes) {
            List<string> keys = [];
            
            // If CliNameAttribute is present, use it
            foreach (CliNameAttribute cliNameAttribute in cliNameAttributes) {
                keys.Add(cliNameAttribute.Name);
            }

            // Otherwise, return the name as-is
            if (keys.Count == 0) {
                keys.Add(name);
            }

            return keys;
        }
    }
}
