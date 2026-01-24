using System.Text.Json.Serialization;

namespace Web.Components.Features.Articles.Models;

/// <summary>
/// Concrete response returned from API when a concurrency conflict occurs while updating an article.
/// </summary>
public sealed class ConcurrencyConflictResponseDto
{
	/// <summary>
	/// Default constructor for JSON deserialization.
	/// </summary>
	public ConcurrencyConflictResponseDto()
	{
		Error = null;
		Code = 0;
		ServerVersion = 0;
		ServerArticle = null;
		ChangedFields = new List<string>();
	}

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
	[JsonPropertyName("error")]
	public string? Error { get; set; }

	/// <summary>
	/// Numeric error code (maps to ResultErrorCode).
	/// </summary>
	[JsonPropertyName("code")]
	public int Code { get; set; }

	/// <summary>
	/// The current server version for the article.
	/// </summary>
	[JsonPropertyName("serverVersion")]
	public int ServerVersion { get; set; }

	/// <summary>
	/// Version property for optimistic concurrency control.
	/// </summary>
	public int Version => ServerVersion;

	/// <summary>
	/// Snapshot of the article on the server (current state).
	/// </summary>
	[JsonPropertyName("serverArticle")]
	public ArticleDto? ServerArticle { get; set; }

	/// <summary>
	/// List of fields that differ between the client and server versions.
	/// </summary>
	[JsonPropertyName("changedFields")]
	public IReadOnlyList<string> ChangedFields { get; set; }
}

