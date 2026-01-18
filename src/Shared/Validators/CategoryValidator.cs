// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CategoryValidator.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Shared
// =======================================================

namespace Shared.Validators;

/// <summary>
///   Validator for the <see cref="Category" /> entity.
/// </summary>
public class CategoryValidator : AbstractValidator<Category>
{

	/// <summary>
	///   Initializes a new instance of the <see cref="CategoryValidator" /> class.
	/// </summary>
	public CategoryValidator()
	{

		RuleFor(x => x.Id)
				.NotNull().WithMessage("Id is required")
				.NotEmpty().WithMessage("Id cannot be empty");

		RuleFor(x => x.CategoryName)
				.NotEmpty().WithMessage("Name is required")
				.MaximumLength(80).WithMessage("Name cannot exceed 80 characters");

		RuleFor(x => x.CreatedOn)
				.NotNull().WithMessage("CreatedOn is required")
				.LessThanOrEqualTo(DateTimeOffset.UtcNow).WithMessage("CreatedOn cannot be in the future");

	}

}
