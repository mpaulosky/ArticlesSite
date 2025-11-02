// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CategoryDtoValidator.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Shared
// =======================================================

namespace Shared.Validators;

/// <summary>
///   Validator for the <see cref="CategoryDto" /> data transfer object.
/// </summary>
public class CategoryDtoValidator : AbstractValidator<CategoryDto>
{

	/// <summary>
	///   Initializes a new instance of the <see cref="CategoryDtoValidator" /> class.
	/// </summary>
	public CategoryDtoValidator()
	{

		RuleFor(x => x.CategoryName)
				.NotEmpty().WithMessage("Name is required")
				.MaximumLength(80).WithMessage("Category name cannot exceed 80 characters");

	}

}
