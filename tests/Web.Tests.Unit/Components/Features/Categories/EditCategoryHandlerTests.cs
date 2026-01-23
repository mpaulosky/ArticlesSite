using Moq;

namespace Web.Tests.Unit.Components.Features.Categories;

public class EditCategoryHandlerTests
{

	readonly CategoryDtoValidator _categoryDtoValidator = Substitute.For<CategoryDtoValidator>();

	public EditCategoryHandlerTests()
	{
		_categoryDtoValidator = _categoryDtoValidator ?? throw new ArgumentNullException(nameof(_categoryDtoValidator));
	}

	[Fact]
	public async Task HandleAsync_Should_Edit_Category()
	{
		var repoMock = new Mock<ICategoryRepository>();
		var category = new Category { Id = ObjectId.GenerateNewId(), CategoryName = "Test", IsArchived = false };
		repoMock.Setup(r => r.GetCategoryByIdAsync(category.Id)).ReturnsAsync(Result.Ok(category));
		var loggerMock = new Mock<ILogger<EditCategory.Handler>>();

		var handler = new EditCategory.Handler(repoMock.Object, loggerMock.Object, _categoryDtoValidator);
		var dto = new CategoryDto { Id = category.Id, CategoryName = "Test", IsArchived = false };
		var result = await handler.HandleAsync(dto);
		Assert.True(result.Success);
		Assert.NotNull(result.Value);
		Assert.Equal("Test", result.Value.CategoryName);
	}

	[Fact]
	public async Task HandleAsync_Should_Fail_When_DtoIsNull()
	{
		var repoMock = new Mock<ICategoryRepository>();
		var loggerMock = new Mock<ILogger<EditCategory.Handler>>();

		var handler = new EditCategory.Handler(repoMock.Object, loggerMock.Object, _categoryDtoValidator);
		var result = await handler.HandleAsync(null!);
		Assert.False(result.Success);
		Assert.Equal("Category data cannot be null", result.Error);
	}

	[Fact]
	public async Task HandleAsync_Should_Fail_When_CategoryNameIsEmpty()
	{
		var repoMock = new Mock<ICategoryRepository>();
		var loggerMock = new Mock<ILogger<EditCategory.Handler>>();

		var handler = new EditCategory.Handler(repoMock.Object, loggerMock.Object, _categoryDtoValidator);
		var dto = new CategoryDto { Id = ObjectId.GenerateNewId(), CategoryName = "", IsArchived = false };
		var result = await handler.HandleAsync(dto);
		Assert.False(result.Success);
		Assert.Equal("Category name is required", result.Error);
	}

	[Fact]
	public async Task HandleAsync_Should_Fail_When_CategoryNotFound()
	{
		var repoMock = new Mock<ICategoryRepository>();
		repoMock.Setup(r => r.GetCategoryByIdAsync(It.IsAny<ObjectId>())).ReturnsAsync(Result.Fail<Category>("not found"));
		var loggerMock = new Mock<ILogger<EditCategory.Handler>>();
		var handler = new EditCategory.Handler(repoMock.Object, loggerMock.Object, _categoryDtoValidator);
		var dto = new CategoryDto { Id = ObjectId.GenerateNewId(), CategoryName = "Test", IsArchived = false };
		var result = await handler.HandleAsync(dto);
		Assert.False(result.Success);
		Assert.Equal("not found", result.Error);
	}
}
