// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     Category.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Shared
// =======================================================

namespace Shared.Entities;

/// <summary>
///   Represents a blog category that can be assigned to posts.
/// </summary>
[Serializable]
public class Category
{

	public Category()
	{
		Id = ObjectId.GenerateNewId();
		CategoryName = string.Empty;
		Slug = string.Empty;
		CreatedOn = null;
		ModifiedOn = null;
		IsArchived = false;
	}

	/// <summary>
	///   Gets the unique identifier for this entity.
	/// </summary>
	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	[BsonElement("_id")]
	public ObjectId Id { get; init; } = ObjectId.GenerateNewId();

	/// <summary>
	///   The name of the category.
	/// </summary>
	[ DisplayName("Category Name")]
	[ MaxLength(80)]
	public string CategoryName { get; set; }

	/// <summary>
	///   Gets or sets the slug for the category, used in the category's URL.
	/// </summary>
	[BsonElement("slug")]
	[BsonRepresentation(BsonType.String)]
	[MaxLength(200)]
	[RegularExpression(@"^[a-z0-9_]+$",
			ErrorMessage = "Slug can only contain lowercase letters, numbers, and underscores")]
	public string Slug { get; set; }

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

	public static Category Empty =>
			new()
			{
					Id = ObjectId.Empty,
					CategoryName = string.Empty,
					Slug = string.Empty,
					CreatedOn = null,
					ModifiedOn = null,
					IsArchived = false
			};

	/// <summary>
	///   Updates the category with new values.
	/// </summary>
	public void Update(string categoryName, string slug, bool isArchived)
	{
		if (string.IsNullOrWhiteSpace(categoryName))
		{
			throw new ArgumentException("Category name cannot be null or whitespace.", nameof(categoryName));
		}

		if (string.IsNullOrWhiteSpace(slug))
		{
			throw new ArgumentException("Slug cannot be null or whitespace.", nameof(slug));
		}

		CategoryName = categoryName;
		Slug = slug;
		IsArchived = isArchived;
		ModifiedOn = DateTimeOffset.UtcNow;
	}

}
