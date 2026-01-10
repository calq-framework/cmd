namespace CalqFramework.Cmd.AspNetCore;

/// <summary>
/// Configuration options for CalqCmdController error caching
/// </summary>
public class CalqCmdCacheOptions
{
    /// <summary>
    /// Key prefix for cached error messages (default: "CalqFramework.Cmd.Errors:")
    /// </summary>
    public string ErrorCacheKeyPrefix { get; set; } = "CalqFramework.Cmd.Errors:";

    /// <summary>
    /// Expiration time for cached error messages (default: 1 hour)
    /// </summary>
    public TimeSpan ErrorCacheExpiration { get; set; } = TimeSpan.FromHours(1);
}