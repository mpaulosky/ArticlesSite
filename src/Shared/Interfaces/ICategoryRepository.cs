// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ICategoryRepository.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Shared
// =======================================================

namespace Shared.Interfaces;

public interface ICategoryRepository
{


	Task<Result<Category>> GetCategoryByIdAsync(ObjectId id);

	Task<Result<Category>> GetCategory(string slug);

	Task<Result<IEnumerable<Category>>> GetCategories();

	Task<Result<IEnumerable<Category>>> GetCategories(Expression<Func<Category, bool>> where);

	Task<Result<Category>> AddCategory(Category category);

	Task<Result<Category>> UpdateCategory(Category category);

	Task ArchiveCategory(string slug);

}
