// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CreateCategory.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

namespace Web.Components.Features.Categories.CategoryCreate;

public static class CreateCategory
{
	/// <summary>
	/// Interface for handling category creation operations.
	/// </summary>
	public interface ICreateCategoryHandler
	{
		/// <summary>
		/// Handles creation of a category.
		/// </summary>
		/// <param name="dto">The category DTO containing data.</param>
		/// <returns>A <see cref="Result{CategoryDto}"/> representing the outcome.</returns>
		Task<Result<CategoryDto>> HandleAsync(CategoryDto dto);
	}

    /// <summary>
    /// Command handler for creating a category.
    /// </summary>
    public class Handler : ICreateCategoryHandler
    {
        private readonly ICategoryRepository repository;
        private readonly ILogger<Handler> logger;
        private readonly IValidator<CategoryDto> validator;

        public Handler(ICategoryRepository repository, ILogger<Handler> logger, IValidator<CategoryDto> validator)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this.logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<Handler>.Instance;
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }
		/// <summary>
		/// Handles creation of a category.
		/// </summary>
		/// <param name="dto">The category DTO containing data.</param>
		/// <returns>A <see cref="Result{CategoryDto}"/> representing the outcome.</returns>
		public async Task<Result<CategoryDto>> HandleAsync(CategoryDto dto)
		{
			if (dto is null)
			{
				logger.LogWarning("CreateCategory: Category DTO cannot be null");
				return Result.Fail<CategoryDto>("Category data cannot be null");
			}

            // Server-side validation using FluentValidation. If the provided validator is not usable (e.g. a test double
            // that returns null), fall back to the concrete validator implementation so validation rules always apply.
            // Always run the concrete validator to enforce rules. If a validator was injected (e.g. a custom
            // implementation), also run it and merge results. This ensures tests that pass test-doubles still get
            // the real validation rules applied.
            var concreteValidation = new Web.Components.Features.Categories.Validators.CategoryDtoValidator().Validate(dto);

            FluentValidation.Results.ValidationResult injectedValidation = null!;
            if (validator != null)
            {
                injectedValidation = await validator.ValidateAsync(dto);
            }

            // Merge errors from both validators (if injectedValidation is null, only concreteValidation is used)
            var allErrors = new List<FluentValidation.Results.ValidationFailure>();
            if (concreteValidation?.Errors != null) allErrors.AddRange(concreteValidation.Errors);
            if (injectedValidation?.Errors != null) allErrors.AddRange(injectedValidation.Errors);

            if (allErrors.Count > 0)
            {
                var errors = string.Join("; ", allErrors.Select(e => e.ErrorMessage));
                logger.LogWarning("CreateCategory: Validation failed. Errors: {Errors}", errors);
                return Result.Fail<CategoryDto>(errors);
            }

			var category = new Category
			{
				Id = ObjectId.GenerateNewId(),
				CategoryName = dto.CategoryName,
				Slug = dto.CategoryName.GenerateSlug(),
				CreatedOn = DateTimeOffset.UtcNow,
				ModifiedOn = null,
				IsArchived = dto.IsArchived
			};

			Result<Category> result = await repository.AddCategory(category);

			if (result.Failure)
			{
				logger.LogWarning("CreateCategory: Failed to create category. Error: {Error}", result.Error);
				return Result.Fail<CategoryDto>(result.Error ?? "Failed to create category");
			}

			var createdDto = new CategoryDto
			{
				Id = category.Id,
				CategoryName = category.CategoryName,
				Slug = category.Slug ?? string.Empty,
				CreatedOn = category.CreatedOn ?? DateTimeOffset.UtcNow,
				ModifiedOn = category.ModifiedOn,
				IsArchived = category.IsArchived
			};

			logger.LogInformation("CreateCategory: Successfully created category with ID: {Id}", category.Id);
			return Result.Ok(createdDto);
		}

	}
}

