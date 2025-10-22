// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CategoryDtoValidator.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Shared
// =======================================================

namespace Shared.Validators;

/// <summary>
///   Validator for the <see cref="Category" /> entity.
/// </summary>
public class CategoryDtoValidator : AbstractValidator<CategoryDto>
{

	/// <summary>
	///   Initializes a new instance of the <see cref="CategoryValidator" /> class.
	/// </summary>
	public CategoryDtoValidator()
	{

		RuleFor(x => x.Id)
				.NotNull()
				.WithMessage("Id is required");

		RuleFor(x => x.CategoryName)
				.NotEmpty().WithMessage("Name is required")
				.MaximumLength(80);

	}

}