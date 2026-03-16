namespace CalqFramework.Cmd.AspNetCore;

/// <summary>
///     Default command executor implementation using CalqFramework.Cli for command-line style parsing.
///     Uses AsIsClassMemberStringifier to preserve original method and parameter names without transformation.
/// </summary>
/// <remarks>
///     Initializes a new instance of CalqCommandExecutor with the specified command target.
/// </remarks>
/// <param name="commandTarget">The target object containing the methods to execute.</param>
public class CalqCommandExecutor(object commandTarget) : ICalqCommandExecutor {
    private readonly object _commandTarget = commandTarget;

    /// <summary>
    ///     Executes a command using CLI-style argument parsing via CalqFramework.Cli with interface output capture.
    /// </summary>
    /// <param name="args">Command-line arguments (e.g., ["MethodName", "--param", "value"]).</param>
    /// <param name="interfaceOut">Optional TextWriter to capture interface output. If null, uses Console.Out.</param>
    /// <returns>The result of the command execution.</returns>
    public object? Execute(string[] args, TextWriter interfaceOut) {
        CommandLineInterface cli = new() {
            InterfaceOut = interfaceOut,
            CliComponentStoreFactory = new CliComponentStoreFactory {
                ClassMemberStringifier = new AsIsClassMemberStringifier()
            }
        };
        return cli.Execute(_commandTarget, args);
    }

    /// <summary>
    ///     Returns class member names as-is without any transformation.
    ///     Preserves original naming conventions (e.g., PascalCase, snake_case).
    /// </summary>
    private class AsIsClassMemberStringifier : ClassMemberStringifierBase {
        protected override IEnumerable<string> GetAlternativeNames(string name, IEnumerable<CliNameAttribute> cliNameAttributes) =>
            // Return empty list - no alternative names
            [];

        protected override IEnumerable<string> GetRequiredNames(string name, IEnumerable<CliNameAttribute> cliNameAttributes) {
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
