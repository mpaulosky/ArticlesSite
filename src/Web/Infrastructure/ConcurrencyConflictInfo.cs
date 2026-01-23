namespace Web.Infrastructure;

/// <summary>
/// Information returned when a concurrency conflict occurs.
/// Contains the server's current version and a snapshot of the article on the server.
/// </summary>
public sealed class ConcurrencyConflictInfo
{
    public ConcurrencyConflictInfo(int serverVersion, Web.Components.Features.Articles.Models.ArticleDto? serverArticle, IEnumerable<string>? changedFields = null)
    {
        ServerVersion = serverVersion;
        ServerArticle = serverArticle;
        ChangedFields = changedFields?.ToList() ?? new List<string>();
    }

    public int ServerVersion { get; }

    public Web.Components.Features.Articles.Models.ArticleDto? ServerArticle { get; }

    public IReadOnlyList<string> ChangedFields { get; }
}
