// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ArticleDto.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Shared
// =======================================================

namespace Shared.Models;

/// <summary>
///   Data Transfer Object (DTO) representing an article.
///   All validations are handled by <see cref="ArticleDtoValidator" />.
/// </summary>
public sealed class ArticleDto
{

	public ArticleDto()
	{
		Id = ObjectId.Empty;
		Slug = string.Empty;
		Title = string.Empty;
		Introduction = string.Empty;
		Content = string.Empty;
		CoverImageUrl = string.Empty;
		Author = null;
		Category = null;
		IsPublished = false;
		PublishedOn = null;
		CreatedOn = null;
		ModifiedOn = null;
		IsArchived = false;
		CanEdit = false;
	}

	public ArticleDto(
			ObjectId id,
			string slug,
			string title,
			string introduction,
			string content,
			string coverImageUrl,
			AuthorInfo? author,
			Category? category,
			bool isPublished,
			DateTimeOffset? publishedOn,
			DateTimeOffset? createdOn,
			DateTimeOffset? modifiedOn,
			bool isArchived,
			bool canEdit)
	{
		Id = id;
		Slug = slug;
		Title = title;
		Introduction = introduction;
		Content = content;
		CoverImageUrl = coverImageUrl;
		Author = author;
		Category = category;
		IsPublished = isPublished;
		PublishedOn = publishedOn;
		CreatedOn = createdOn;
		ModifiedOn = modifiedOn;
		IsArchived = isArchived;
		CanEdit = canEdit;
	}

	/// <summary>
	///   Gets the unique identifier for this entity.
	/// </summary>
	[BsonId]
	[Required(ErrorMessage = "An Id is required")]
	[BsonRepresentation(BsonType.ObjectId)]
	[BsonElement("_id")]
	public ObjectId Id { get; init; }

	/// <summary>
	///   Gets or sets the slug for the article, used in the article's URL.
	/// </summary>
	[BsonElement("Slug")]
	[BsonRepresentation(BsonType.String)]
	[MaxLength(200)]
	[RegularExpression(@"^[a-z0-9_]+$",
			ErrorMessage = "URL slug can only contain lowercase letters, numbers, and underscores")]
	public string Slug { get; set; }

	/// <summary>
	///   Gets or sets the title of the article.
	/// </summary>
	[BsonElement("title")]
	[BsonRepresentation(BsonType.String)]
	[MaxLength(100)]
	public string Title { get; set; }

	/// <summary>
	///   Gets or sets the introduction of the article.
	/// </summary>
	[BsonElement("introduction")]
	[BsonRepresentation(BsonType.String)]
	[MaxLength(200)]
	public string Introduction { get; set; }

	/// <summary>
	///   Gets or sets the main content of the article.
	/// </summary>
	[BsonElement("content")]
	[BsonRepresentation(BsonType.String)]
	[MaxLength(10000)]
	public string Content { get; set; }

	/// <summary>
	///   Gets or sets the URL of the cover image for the article.
	/// </summary>
	[BsonElement("coverImageUrl")]
	[BsonRepresentation(BsonType.String)]
	[MaxLength(200)]
	public string CoverImageUrl { get; set; }

	/// <summary>
	///   Represents the author information associated with the article.
	/// </summary>
	[BsonElement("author")]
	public AuthorInfo? Author { get; set; }

	/// <summary>
	///   Represents the category entity associated with the article.
	/// </summary>
	[BsonElement("category")]
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
	[BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
	[BsonElement("createdOn")]
	public DateTimeOffset? CreatedOn { get; set; }

	/// <summary>
	///   Gets or sets the date and time when this entity was last modified.
	/// </summary>
	[DisplayName("Modified On")]
	[BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
	[BsonElement("modifiedOn")]
	public DateTimeOffset? ModifiedOn { get; set; }

	/// <summary>
	///   Gets or sets a value indicating whether the article is archived.
	/// </summary>
	[DisplayName("Is Archived")]
	[BsonRepresentation(BsonType.Boolean)]
	[BsonElement("isArchived")]
	public bool IsArchived { get; set; }


	[DisplayName("Can Edit")]
	[BsonRepresentation(BsonType.Boolean)]
	[BsonElement("canEdit")]
	public bool CanEdit { get; set; }

	/// <summary>
	///   Gets or sets the URL slug (alias for Slug property).
	/// </summary>
	[BsonIgnore]
	public string UrlSlug
	{
		get => Slug;

		set => Slug = value;
	}

	/// <summary>
	///   Gets an empty ArticleDto instance.
	/// </summary>
	public static ArticleDto Empty =>
			new()
			{
				Id = ObjectId.Empty,
				Slug = string.Empty,
				Title = string.Empty,
				Introduction = string.Empty,
				Content = string.Empty,
				CoverImageUrl = string.Empty,
				Author = null,
				Category = null,
				IsPublished = false,
				PublishedOn = null,
				CreatedOn = null,
				ModifiedOn = null,
				IsArchived = false,
				CanEdit = false
			};

}