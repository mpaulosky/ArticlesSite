namespace Web.Components.Features.Articles.Models;

/// <summary>
/// Concrete response returned from API when a concurrency conflict occurs while updating an article.
/// </summary>
public sealed class ConcurrencyConflictResponseDto
{
    public ConcurrencyConflictResponseDto(string? error, int code, int serverVersion, ArticleDto? serverArticle, IEnumerable<string>? changedFields)
    {
        Error = error;
        Code = code;
        ServerVersion = serverVersion;
        ServerArticle = serverArticle;
        ChangedFields = changedFields?.ToList() ?? new List<string>();
    }

    /// <summary>
    /// Error message describing the conflict.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Numeric error code (maps to ResultErrorCode).
    /// </summary>
    public int Code { get; }

    /// <summary>
    /// The current server version for the article.
    /// </summary>
    public int ServerVersion { get; }

    /// <summary>
    /// Snapshot of the article on the server (current state).
    /// </summary>
    public ArticleDto? ServerArticle { get; }

    /// <summary>
    /// List of fields that differ between the client and server versions.
    /// </summary>
    public IReadOnlyList<string> ChangedFields { get; }
}
