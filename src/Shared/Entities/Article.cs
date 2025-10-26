// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     Article.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Shared
// =======================================================

namespace Shared.Entities;

/// <summary>
///   Represents an article in the blog system.
/// </summary>
[Serializable]
public class Article
{

	public Article()
	{
		Title = string.Empty;
		Introduction = string.Empty;
		Content = string.Empty;
		CoverImageUrl = string.Empty;
		Slug = string.Empty;
		CreatedOn = DateTimeOffset.UtcNow;
		Author = null;
		Category = null;
		IsPublished = false;
		PublishedOn = null;
		IsArchived = false;
		ModifiedOn = null;
		Author = null;
	}

	public Article(
			string title,
			string introduction,
			string content,
			string? coverImageUrl,
			AuthorInfo? author,
			Category? category) : this()
	{
		if (string.IsNullOrWhiteSpace(title))
		{
			throw new ArgumentException("Title cannot be null or whitespace.", nameof(title));
		}

		if (string.IsNullOrWhiteSpace(introduction))
		{
			throw new ArgumentException("Introduction cannot be null or whitespace.", nameof(introduction));
		}

		if (string.IsNullOrWhiteSpace(content))
		{
			throw new ArgumentException("Content cannot be null or whitespace.", nameof(content));
		}

		Title = title;
		Introduction = introduction;
		Content = content;
		CoverImageUrl = coverImageUrl ?? "https://example.com/image.jpg";
		Slug = GenerateSlug(title);
		Author = author;
		Category = category;
		IsPublished = false;
		PublishedOn = null;
		IsArchived = false;
	}

	public Article(
			string title,
			string introduction,
			string content,
			string? coverImageUrl,
			AuthorInfo? author,
			Category? category,
			bool isPublished,
			DateTimeOffset? publishedOn,
			bool isArchived)
			: this(title, introduction, content, coverImageUrl, author, category)
	{
		IsPublished = isPublished;
		PublishedOn = publishedOn;
		IsArchived = isArchived;
	}

	/// <summary>
	///   Gets the unique identifier for this entity.
	/// </summary>
	[BsonId]
	[Required(ErrorMessage = "Id is required")]
	[BsonRepresentation(BsonType.ObjectId)]
	[BsonElement("_id")]
	public ObjectId Id { get; init; } = ObjectId.GenerateNewId();

	/// <summary>
	///   Gets or sets the slug for the article, used in the article's URL.
	/// </summary>
	[BsonElement("Slug")]
	[BsonRepresentation(BsonType.String)]
	[Required(ErrorMessage = "Slug is required")]
	[MaxLength(200)]
	[RegularExpression(@"^[a-z0-9_]+$",
			ErrorMessage = "Slug can only contain lowercase letters, numbers, and underscores")]
	public string Slug { get; private set; }

	/// <summary>
	///   Gets or sets the title of the article.
	/// </summary>
	[BsonElement("title")]
	[BsonRepresentation(BsonType.String)]
	[Required(ErrorMessage = "Title is required")]
	[MaxLength(100)]
	public string Title { get; set; }

	/// <summary>
	///   Gets or sets the introduction of the article.
	/// </summary>
	[BsonElement("introduction")]
	[BsonRepresentation(BsonType.String)]
	[Required(ErrorMessage = "Introduction is required")]
	[MaxLength(200)]
	public string Introduction { get; set; }

	/// <summary>
	///   Gets or sets the main content of the article.
	/// </summary>
	[BsonElement("content")]
	[BsonRepresentation(BsonType.String)]
	[Required(ErrorMessage = "Content is required")]
	[MaxLength(10000)]
	public string Content { get; set; }

	/// <summary>
	///   Gets or sets the URL of the cover image for the article.
	/// </summary>
	[BsonElement("coverImageUrl")]
	[BsonRepresentation(BsonType.String)]
	[Required(ErrorMessage = "Cover image is required")]
	[MaxLength(200)]
	public string CoverImageUrl { get; set; }

	/// <summary>
	///   Represents the author information associated with the article.
	/// </summary>
	[BsonElement("author")]
	[Required(ErrorMessage = "Author is required")]
	public AuthorInfo? Author { get; set; }

	/// <summary>
	///   Represents the category entity associated with the article.
	/// </summary>
	[BsonElement("category")]
	[Required(ErrorMessage = "Category is required")]
	public Category? Category { get; set; }

	/// <summary>
	///   Gets or sets a value indicating whether the article is published.
	/// </summary>
	[BsonElement("isPublished")]
	[BsonRepresentation(BsonType.Boolean)]
	[DisplayName("Is Published")]
	public bool IsPublished { get; set; }

	/// <summary>
	///   Gets or sets the date and time when the article was published.
	/// </summary>
	[BsonElement("publishedOn")]
	[BsonRepresentation(BsonType.DateTime)]
	[DisplayName("Published On")]
	public DateTimeOffset? PublishedOn { get; set; }

	/// <summary>
	///   Gets the date and time when this entity was created.
	/// </summary>
	[DisplayName("Created On")]
	[BsonElement("createdOn")]
	public DateTimeOffset? CreatedOn { get; set; }

	/// <summary>
	///   Gets or sets the date and time when this entity was last modified.
	/// </summary>
	[DisplayName("Modified On")]
	[BsonElement("modifiedOn")]
	public DateTimeOffset? ModifiedOn { get; set; }

	/// <summary>
	///   Gets or sets a value indicating whether the article is archived.
	/// </summary>
	[DisplayName("Is Archived")]
	[BsonRepresentation(BsonType.Boolean)]
	[BsonElement("isArchived")]
	public bool IsArchived { get; set; }

	/// <summary>
	///   Gets an empty article instance.
	/// </summary>
	public static Article Empty =>
			new()
			{
					Id = ObjectId.Empty,
					Title = string.Empty,
					Introduction = string.Empty,
					Content = string.Empty,
					CoverImageUrl = string.Empty,
					Slug = string.Empty,
					Author = null,
					Category = null,
					IsPublished = false,
					PublishedOn = null,
					IsArchived = false
			};

	/// <summary>
	///   Updates the article with new values.
	/// </summary>
	public void Update(
			string title,
			string introduction,
			string content,
			string coverImageUrl,
			bool isPublished,
			DateTimeOffset? publishedOn,
			bool isArchived)
	{
		if (string.IsNullOrWhiteSpace(title))
		{
			throw new ArgumentException("Title cannot be null or whitespace.", nameof(title));
		}

		if (string.IsNullOrWhiteSpace(introduction))
		{
			throw new ArgumentException("Introduction cannot be null or whitespace.", nameof(introduction));
		}

		if (string.IsNullOrWhiteSpace(content))
		{
			throw new ArgumentException("Content cannot be null or whitespace.", nameof(content));
		}

		Title = title;
		Introduction = introduction;
		Content = content;
		CoverImageUrl = coverImageUrl;
		Slug = GenerateSlug(title);
		IsPublished = isPublished;
		PublishedOn = publishedOn;
		IsArchived = isArchived;
		ModifiedOn = DateTimeOffset.UtcNow;
	}

	/// <summary>
	///   Publishes the article.
	/// </summary>
	public void Publish(DateTimeOffset publishedOn)
	{
		IsPublished = true;
		PublishedOn = publishedOn;
		ModifiedOn = DateTimeOffset.UtcNow;
	}

	/// <summary>
	///   Unpublishes the article.
	/// </summary>
	public void Unpublish()
	{
		IsPublished = false;
		PublishedOn = null;
		ModifiedOn = DateTimeOffset.UtcNow;
	}

	private static string GenerateSlug(string title)
	{
		if (string.IsNullOrWhiteSpace(title))
		{
			throw new ArgumentException("Title cannot be null or whitespace.", nameof(title));
		}

		// Lowercase, replace non-alphanumeric sequences with single underscore, collapse multiple underscores
		string slug = title.ToLowerInvariant();

		// Replace any sequence of non-alphanumeric characters with underscore
		slug = Regex.Replace(slug, "[^a-z0-9]+", "_");

		// Collapse multiple underscores into one
		slug = Regex.Replace(slug, "_+", "_");

		// Trim leading/trailing underscores
		slug = slug.Trim('_');

		return slug;
	}

}