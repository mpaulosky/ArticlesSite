// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ArticleDtoValidator.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Shared
// =======================================================

namespace Shared.Validators;

/// <summary>
///   Validator for the <see cref="ArticleDto" /> model.
/// </summary>
public class ArticleDtoValidator : AbstractValidator<ArticleDto>
{

	public ArticleDtoValidator()
	{
		RuleFor(x => x.Id)
				.NotNull()
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
				.WithMessage("URL slug is required")
				.MaximumLength(200)
				.Matches(@"^[a-z0-9_]+$")
				.WithMessage("URL slug can only contain lowercase letters, numbers, and underscores");

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
				.MaximumLength(4000)
				.WithMessage("Content cannot exceed 4000 characters");
	}

}
