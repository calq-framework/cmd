// NOTE: This example uses Calq CLI for the CLI interface.
// See https://github.com/calq-framework/cli for full Calq CLI documentation.

using System.Text.Json;
using CalqFramework.Cli;
using CalqFramework.Cmd.Shells;
using Example.CliTool.KubectlWrapper;
using static CalqFramework.Cmd.Terminal;

LocalTerminal.Shell = new Bash();

try {
    object? result = new CommandLineInterface().Execute(new Kubectl());
    if (result is not ValueTuple) Console.WriteLine(JsonSerializer.Serialize(result));
} catch (CliException ex) {
    Console.Error.WriteLine(ex.Message);
    Environment.Exit(1);
}

namespace Example.CliTool.KubectlWrapper {
    /// <summary>Kubectl wrapper — a CLI tool built with Calq CLI and Calq CMD.</summary>
    class Kubectl {
        /// <summary>Namespace to operate in.</summary>
        public string Namespace { get; set; } = "default";

        /// <summary>Get cluster resources.</summary>
        public string Get(string resource) =>
            CMD($"kubectl get {resource} -n {Namespace} -o json");

        /// <summary>Apply a manifest file.</summary>
        public void Apply(string file) =>
            RUN($"kubectl apply -f {file} -n {Namespace}");

        /// <summary>Delete a resource.</summary>
        public void Delete(string resource, string name) =>
            RUN($"kubectl delete {resource} {name} -n {Namespace}");

        /// <summary>View pod logs.</summary>
        public void Logs(string pod, bool follow = false) =>
            RUN($"kubectl logs {pod} -n {Namespace}{(follow ? " -f" : "")}");

        /// <summary>Scale a deployment.</summary>
        public void Scale(string deployment, int replicas) =>
            RUN($"kubectl scale deployment {deployment} --replicas={replicas} -n {Namespace}");
    }
}
