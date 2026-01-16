using Moq;

using Web.Components.Features.Categories.CategoriesList;

namespace Web.Tests.Unit.Components.Features.Categories;

public class GetCategoriesHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_Return_Categories()
    {
        var repoMock = new Mock<ICategoryRepository>();
        repoMock.Setup(r => r.GetCategories()).ReturnsAsync(Result.Ok<IEnumerable<Category>>(new List<Category> {
            new Category { Id = ObjectId.GenerateNewId(), CategoryName = "Test", IsArchived = false }
        }));
        var loggerMock = new Mock<ILogger<GetCategories.Handler>>();
        var handler = new GetCategories.Handler(repoMock.Object, loggerMock.Object);
        var result = await handler.HandleAsync();
        Assert.True(result.Success);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value);
        Assert.Equal("Test", result.Value.First().CategoryName);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_Failure_When_RepositoryFails()
    {
        var repoMock = new Mock<ICategoryRepository>();
        repoMock.Setup(r => r.GetCategories()).ReturnsAsync(Result.Fail<IEnumerable<Category>>("db error"));
        var loggerMock = new Mock<ILogger<GetCategories.Handler>>();
        var handler = new GetCategories.Handler(repoMock.Object, loggerMock.Object);
        var result = await handler.HandleAsync();
        Assert.False(result.Success);
        Assert.Equal("db error", result.Error);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_Failure_When_RepositoryReturnsNull()
    {
        var repoMock = new Mock<ICategoryRepository>();
        repoMock.Setup(r => r.GetCategories()).ReturnsAsync(Result.Ok<IEnumerable<Category>>(default!));
        var loggerMock = new Mock<ILogger<GetCategories.Handler>>();
        var handler = new GetCategories.Handler(repoMock.Object, loggerMock.Object);
        var result = await handler.HandleAsync();
        Assert.False(result.Success);
        Assert.Equal("No categories found", result.Error);
    }

    [Fact]
    public async Task HandleAsync_Should_Filter_Archived_Categories()
    {
        var repoMock = new Mock<ICategoryRepository>();
        var categories = new List<Category> {
            new Category { Id = ObjectId.GenerateNewId(), CategoryName = "Active", IsArchived = false },
            new Category { Id = ObjectId.GenerateNewId(), CategoryName = "Archived", IsArchived = true }
        };
        repoMock.Setup(r => r.GetCategories()).ReturnsAsync(Result.Ok<IEnumerable<Category>>(categories));
        var loggerMock = new Mock<ILogger<GetCategories.Handler>>();
        var handler = new GetCategories.Handler(repoMock.Object, loggerMock.Object);
        var result = await handler.HandleAsync();
        Assert.True(result.Success);
        Assert.Single(result.Value!);
        Assert.Equal("Active", result.Value!.First().CategoryName);
    }
}
