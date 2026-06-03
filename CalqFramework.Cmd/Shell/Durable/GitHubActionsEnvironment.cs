namespace CalqFramework.Cmd.Shell.Durable;

/// <summary>
///     Captures GitHub Actions runtime context from environment variables.
///     ACTIONS_CACHE_URL + ACTIONS_RUNTIME_TOKEN: internal cache API credentials (auto-available).
///     GITHUB_TOKEN: public REST API for cache deletion on success (auto-available).
///     GITHUB_SHA: commit scope for cache isolation.
///     Self-validating: if ACTIONS_CACHE_URL or ACTIONS_RUNTIME_TOKEN is missing,
///     the cache API is unusable — detection falls through to filesystem.
/// </summary>
internal sealed class GitHubActionsEnvironment {
    public required string CacheUrl { get; init; }
    public required string RuntimeToken { get; init; }
    public string? GitHubToken { get; init; }
    public string? Repository { get; init; }
    public string? CommitSha { get; init; }

    public static bool IsDetected() =>
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ACTIONS_CACHE_URL")) && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ACTIONS_RUNTIME_TOKEN"));

    public static GitHubActionsEnvironment FromCurrent() => new() {
        CacheUrl = Environment.GetEnvironmentVariable("ACTIONS_CACHE_URL")!,
        RuntimeToken = Environment.GetEnvironmentVariable("ACTIONS_RUNTIME_TOKEN")!,
        GitHubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN"),
        Repository = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY"),
        CommitSha = Environment.GetEnvironmentVariable("GITHUB_SHA")
    };
}
