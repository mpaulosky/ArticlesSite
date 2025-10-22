namespace Shared.Interfaces;

public interface ICategoryRepository
{
	Task<Result<Category?>> GetCategoryByIdAsync(ObjectId id);
	Task<Result<Category?>> GetCategory(string slug);
	Task<Result<IEnumerable<Category>?>> GetCategories();
	Task<Result<IEnumerable<Category>?>> GetCategories(Expression<Func<Category, bool>> where);
	Task<Result<Category>> AddCategory(Category category);
	Task<Result<Category>> UpdateCategory(Category category);
	Task ArchiveCategory(string slug);
}