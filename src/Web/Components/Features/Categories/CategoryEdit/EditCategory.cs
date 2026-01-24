// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     EditCategory.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

namespace Web.Components.Features.Categories.CategoryEdit;

public static class EditCategory
{

	/// <summary>
	/// Interface for handling category edit operations.
	/// </summary>
	public interface IEditCategoryHandler
	{

		/// <summary>
		/// Handles the edit operation for a category.
		/// </summary>
		/// <param name="dto">The category DTO containing updated data.</param>
		/// <returns>A <see cref="Result{CategoryDto}"/> representing the outcome of the edit operation.</returns>
		Task<Result<CategoryDto>> HandleAsync(CategoryDto dto);

	}

	/// <summary>
	/// Command handler for editing a category.
	/// </summary>
	public class Handler : IEditCategoryHandler
	{
		private readonly ICategoryRepository _repository;
		private readonly ILogger<Handler> _logger;
		private readonly FluentValidation.IValidator<CategoryDto>? _validator;
		private readonly Web.Infrastructure.ConcurrencyOptions _concurrencyOptions;
		private readonly IAsyncPolicy<Result<Category>> _concurrencyPolicy;

		public Handler(ICategoryRepository repository, ILogger<Handler>? logger, FluentValidation.IValidator<CategoryDto>? validator, Microsoft.Extensions.Options.IOptions<Web.Infrastructure.ConcurrencyOptions>? concurrencyOptions = null, IAsyncPolicy<Result<Category>>? concurrencyPolicy = null)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<Handler>.Instance;
			_validator = validator;
			_concurrencyOptions = concurrencyOptions?.Value ?? new Web.Infrastructure.ConcurrencyOptions();
			_concurrencyPolicy = concurrencyPolicy ?? Web.Infrastructure.ConcurrencyPolicies.CreatePolicy<Category>(_concurrencyOptions);
		}

		/// <inheritdoc />
		public async Task<Result<CategoryDto>> HandleAsync(CategoryDto dto)
		{
			if (_repository == null) throw new InvalidOperationException("Repository unset in EditCategory.Handler");
			if (_logger == null) throw new InvalidOperationException("Logger unset in EditCategory.Handler");

			if (dto is null)
			{
				_logger.LogInformation("EditCategory: Category DTO cannot be null");

				return Result.Fail<CategoryDto>("Category data cannot be null");
			}

			// Validate incoming DTO using FluentValidation rules. Merge injected validator with concrete validator
			var concreteValidation = new Web.Components.Features.Categories.Validators.CategoryDtoValidator().Validate(dto);
			FluentValidation.Results.ValidationResult injectedValidation = null!;
			if (_validator != null)
			{
				injectedValidation = await _validator.ValidateAsync(dto);
			}

			var allErrors = new List<FluentValidation.Results.ValidationFailure>();
			if (concreteValidation?.Errors != null) allErrors.AddRange(concreteValidation.Errors);
			if (injectedValidation?.Errors != null) allErrors.AddRange(injectedValidation.Errors);

			if (allErrors.Count > 0)
			{
				var errors = string.Join("; ", allErrors.Select(e => e.ErrorMessage));
				_logger.LogInformation("EditCategory: Validation failed. Errors: {Errors}", errors);
				return Result.Fail<CategoryDto>(errors);
			}

			Result<Category> existingResult = await _repository.GetCategoryByIdAsync(dto.Id);

			if (existingResult.Failure)
			{
				_logger.LogInformation("EditCategory: Category not found with ID: {Id}", dto.Id);

				return Result.Fail<CategoryDto>(existingResult.Error ?? "Category not found");
			}

			if (existingResult.Value is null)
			{
				_logger.LogInformation("EditCategory: Category not found with ID: {Id}", dto.Id);

				return Result.Fail<CategoryDto>("Category not found");
			}

			Category category = existingResult.Value;
			string slug = dto.CategoryName.GenerateSlug();
			try
			{
				// Use the IsArchived state from the DTO
				category.Update(dto.CategoryName, slug, dto.IsArchived);
			}
			catch (ArgumentException ex)
			{
				_logger.LogInformation(ex, "EditCategory: Validation failed during update for category {Id}", dto.Id);
				return Result.Fail<CategoryDto>(ex.Message);
			}

			// Use centralized Polly policy to execute update with retries. We provide an onRetryAction via Context
			var context = new Polly.Context();
			context["onRetryAction"] = (Func<Task>)(async () =>
			{
				_logger.LogDebug("EditCategory: onRetryAction invoked for category {Id}", category.Id);
				_logger.LogInformation("EditCategory.onRetryAction invoked for category {Id}. Current local Version={Version}", category.Id, category.Version);
				var latest = await _repository.GetCategoryByIdAsync(category.Id);
				if (latest.Failure || latest.Value is null)
				{
					context["terminalFailure"] = Result.Fail<Category>(latest.Error ?? "Category not found");
					return;
				}

				category = latest.Value;
				_logger.LogInformation("EditCategory.onRetryAction loaded latest category {Id} Version={Version}", category.Id, category.Version);
				try
				{
					// Use the IsArchived state from the DTO
					category.Update(dto.CategoryName, slug, dto.IsArchived);
				}
				catch (ArgumentException ex)
				{
					context["terminalFailure"] = Result.Fail<Category>(ex.Message);
					return;
				}
			});

			_logger.LogInformation("EditCategory.HandleAsync: Calling UpdateCategory for category {Id} with Version={Version} DTO.Version={DtoVersion}", category.Id, category.Version, dto.Version);
			Result<Category> policyResult;
			try
			{
				_logger.LogDebug("EditCategory: Executing concurrency policy for category {Id}", category.Id);
				policyResult = await _concurrencyPolicy.ExecuteAsync((ctx) => _repository.UpdateCategory(category), context);
				_logger.LogDebug("EditCategory: Concurrency policy completed for category {Id}. Success={Success} ErrorCode={ErrorCode}", category.Id, !policyResult.Failure, policyResult.ErrorCode);
				if (!policyResult.Failure && policyResult.Value is not null)
				{
					_logger.LogDebug("EditCategory: Repository returned category version {Version} for category {Id}", policyResult.Value.Version, category.Id);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "EditCategory: Unexpected error during policy execution for category {Id}", category.Id);
				return Result.Fail<CategoryDto>("Failed to update category");
			}

			// After retries check for terminal failure
			if (context.TryGetValue("terminalFailure", out var term) && term is Result<Category> terminal && terminal.Failure)
			{
				_logger.LogInformation("EditCategory: terminal failure for ID={Id} Error={Error} Code={ErrorCode}", category.Id, terminal.Error, terminal.ErrorCode);
				if (terminal.ErrorCode == global::Shared.Abstractions.ResultErrorCode.Concurrency)
				{
					return Result.Fail<CategoryDto>(terminal.Error ?? "Concurrency conflict: category was modified by another process", global::Shared.Abstractions.ResultErrorCode.Concurrency, terminal.Details);
				}
				return Result.Fail<CategoryDto>(terminal.Error ?? "Failed to update category");
			}

			if (policyResult.Failure)
			{
				_logger.LogInformation("EditCategory: policyResult failure for ID={Id} Error={Error} Code={ErrorCode}", category.Id, policyResult.Error, policyResult.ErrorCode);
				if (policyResult.ErrorCode == global::Shared.Abstractions.ResultErrorCode.Concurrency)
				{
					return Result.Fail<CategoryDto>(policyResult.Error ?? "Concurrency conflict: category was modified by another process", global::Shared.Abstractions.ResultErrorCode.Concurrency, policyResult.Details);
				}
				return Result.Fail<CategoryDto>(policyResult.Error ?? "Failed to update category");
			}

			// Use the authoritative category returned by the repository/policy when available
			var authoritative = policyResult.Value ?? category;
			var updatedDto = new CategoryDto
			{
				Id = authoritative.Id,
				CategoryName = authoritative.CategoryName,
				Slug = authoritative.Slug ?? string.Empty,
				CreatedOn = authoritative.CreatedOn ?? DateTimeOffset.UtcNow,
				ModifiedOn = authoritative.ModifiedOn,
				IsArchived = authoritative.IsArchived
			};

			_logger.LogInformation("EditCategory: Successfully updated category with ID: {Id}", authoritative.Id);

			return Result.Ok(updatedDto);
		}

	}

}

