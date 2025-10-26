// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ArticleValidator.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Shared
// =======================================================

namespace Shared.Validators;

/// <summary>
///   Validator for the <see cref="Article" /> entity.
/// </summary>
public class ArticleValidator : AbstractValidator<Article>
{

	public ArticleValidator()
	{

		RuleFor(x => x.Id).NotEqual(ObjectId.Empty)
				.WithMessage("Id is required");

		RuleFor(x => x.Id).NotEmpty()
				.WithMessage("Id is required");

		RuleFor(x => x.Id).NotNull()
				.WithMessage("Id is required");

		RuleFor(x => x.Title)
				.NotEmpty()
				.WithMessage("Title is required")
				.MaximumLength(100);

		RuleFor(x => x.Introduction)
				.NotEmpty()
				.WithMessage("Introduction is required")
				.MaximumLength(200);

		RuleFor(x => x.CoverImageUrl)
				.NotEmpty()
				.WithMessage("Cover image is required")
				.MaximumLength(200);

		RuleFor(x => x.Slug)
				.NotEmpty()
				.WithMessage("Slug is required")
				.MaximumLength(200)
				.Matches(@"^[a-z0-9_]+$")
				.WithMessage("Slug can only contain lowercase letters, numbers, and underscores");

		RuleFor(x => x.Author)
				.NotNull()
				.WithMessage("Author is required");

		RuleFor(x => x.Category)
				.NotNull()
				.WithMessage("Category is required");

		RuleFor(x => x.PublishedOn)
				.NotNull()
				.When(x => x.IsPublished)
				.WithMessage("PublishedOn is required when IsPublished is true");

		RuleFor(x => x.Content)
				.NotEmpty()
				.WithMessage("Content is required")
				.MaximumLength(10000)
				.WithMessage("Content cannot exceed 10000 characters");
	}

}