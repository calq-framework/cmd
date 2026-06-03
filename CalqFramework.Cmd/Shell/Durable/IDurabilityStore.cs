namespace CalqFramework.Cmd.Shell.Durable;

/// <summary>
///     Abstracts persistence for durable shell execution.
///     Stream-based API enables zero-copy for filesystem persistence.
///     Scoped to a single workflow instance at construction time.
/// </summary>
public interface IDurabilityStore : IDisposable {
    /// <summary>Returns a readable stream for the cached entry, or null if not found.</summary>
    Stream? Get(string key);

    /// <summary>
    ///     Returns a writable stream for staging a new entry.
    ///     The entry is not visible to Get until Commit is called.
    /// </summary>
    Stream Create(string key);

    /// <summary>
    ///     Promotes a staged entry to committed state.
    ///     After this call, Get(key) returns the committed data.
    /// </summary>
    void Commit(string key);

    /// <summary>Discards a staged entry. No committed entry is created.</summary>
    void Discard(string key);

    /// <summary>Removes all committed and staged entries in this store's scope.</summary>
    void Clear();
}
