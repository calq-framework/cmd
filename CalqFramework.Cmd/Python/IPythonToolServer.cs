using CalqFramework.Cmd.Shell;

namespace CalqFramework.Cmd.Python {

    /// <summary>
    /// Interface for Python tool servers that provide HTTP/2 endpoints for Python script execution.
    /// Compatible with Python Fire and supports streaming via async generators.
    /// </summary>

    public interface IPythonToolServer {
        /// <summary>
        /// The HTTPS URI where the Python tool server is accessible.
        /// Only available after the server has been started successfully.
        /// </summary>
        Uri Uri { get; }

        /// <summary>
        /// Starts the Python tool server with SSL certificates and HTTP/2 support.
        /// Embeds the Python server script, generates certificates, and begins listening for requests.
        /// Returns a worker that manages the server process lifecycle.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the server startup</param>
        /// <returns>A shell worker managing the Python server process</returns>
        Task<IShellWorker> StartAsync(CancellationToken cancellationToken = default);
    }
}
