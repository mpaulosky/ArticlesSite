# Integration Testing Guide

## Overview

This guide covers integration testing for solutions scaffolded by the solution-scaffolder skill. Integration tests verify that multiple components work together correctly, including database access, API endpoints, and external service integrations.

## Test Framework

Integration tests use:
- **xUnit** for test execution
- **FluentAssertions** for assertions
- **TestContainers** for running real dependencies (databases, message queues, etc.)
- **WebApplicationFactory** for API testing

## Project Structure

Integration tests are organized by feature in the `tests/{SolutionName}.Tests.Integration/` project:

```
tests/
└── YourSolution.Tests.Integration/
    ├── Features/
    │   ├── Articles/
    │   │   ├── CreateArticleEndpointTests.cs
    │   │   └── GetArticlesEndpointTests.cs
    │   └── Comments/
    │       └── CreateCommentEndpointTests.cs
    ├── Infrastructure/
    │   ├── CustomWebApplicationFactory.cs
    │   └── IntegrationTestBase.cs
    └── GlobalUsings.cs
```

## Setting Up Integration Tests

### Base Test Class

Create a base class for integration tests:

```csharp
namespace YourSolution.Tests.Integration.Infrastructure;

/// <summary>
/// Base class for integration tests providing test server and database setup
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
	protected HttpClient Client { get; private set; } = null!;
	protected CustomWebApplicationFactory Factory { get; private set; } = null!;
	
	private MongoDbContainer? _mongoContainer;

	public async Task InitializeAsync()
	{
		// Start MongoDB container
		_mongoContainer = new MongoDbBuilder()
			.WithImage("mongo:7.0")
			.WithPortBinding(27017, true)
			.Build();
		
		await _mongoContainer.StartAsync();
		
		// Create factory with container connection string
		Factory = new CustomWebApplicationFactory(_mongoContainer.GetConnectionString());
		Client = Factory.CreateClient();
	}

	public async Task DisposeAsync()
	{
		Client?.Dispose();
		Factory?.Dispose();
		
		if (_mongoContainer is not null)
		{
			await _mongoContainer.DisposeAsync();
		}
	}
}
```

### Custom Web Application Factory

Create a factory to configure the test server:

```csharp
namespace YourSolution.Tests.Integration.Infrastructure;

/// <summary>
/// Custom factory for integration tests with test database configuration
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
	private readonly string _connectionString;

	public CustomWebApplicationFactory(string connectionString)
	{
		_connectionString = connectionString;
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureTestServices(services =>
		{
			// Remove existing MongoDB registration
			var descriptor = services.SingleOrDefault(
				d => d.ServiceType == typeof(IMongoClient));
			
			if (descriptor is not null)
			{
				services.Remove(descriptor);
			}
			
			// Register test MongoDB client
			services.AddSingleton<IMongoClient>(sp =>
				new MongoClient(_connectionString));
		});
		
		builder.UseEnvironment("Testing");
	}
}
```

## Writing Integration Tests

### API Endpoint Tests

```csharp
namespace YourSolution.Tests.Integration.Features.Articles;

/// <summary>
/// Integration tests for Article API endpoints
/// </summary>
public sealed class CreateArticleEndpointTests : IntegrationTestBase
{
	[Fact]
	public async Task CreateArticle_ValidRequest_ReturnsCreatedArticle()
	{
		// Arrange
		var request = new CreateArticleRequest(
			Title: "Integration Test Article",
			Content: "This is test content",
			AuthorId: Guid.NewGuid()
		);

		// Act
		var response = await Client.PostAsJsonAsync("/api/articles", request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Created);
		
		var article = await response.Content.ReadFromJsonAsync<ArticleResponse>();
		article.Should().NotBeNull();
		article!.Title.Should().Be(request.Title);
		article.Content.Should().Be(request.Content);
		
		response.Headers.Location.Should().NotBeNull();
		response.Headers.Location!.ToString().Should().Contain($"/api/articles/{article.Id}");
	}

	[Fact]
	public async Task CreateArticle_InvalidRequest_ReturnsBadRequest()
	{
		// Arrange
		var request = new CreateArticleRequest(
			Title: "", // Invalid - empty title
			Content: "Content",
			AuthorId: Guid.NewGuid()
		);

		// Act
		var response = await Client.PostAsJsonAsync("/api/articles", request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
		
		var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
		problemDetails.Should().NotBeNull();
		problemDetails!.Errors.Should().ContainKey("Title");
	}

	[Fact]
	public async Task GetArticles_NoArticles_ReturnsEmptyList()
	{
		// Act
		var response = await Client.GetAsync("/api/articles");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		
		var articles = await response.Content.ReadFromJsonAsync<List<ArticleResponse>>();
		articles.Should().NotBeNull().And.BeEmpty();
	}

	[Fact]
	public async Task GetArticles_WithArticles_ReturnsArticlesList()
	{
		// Arrange - Create test data
		var article1 = new CreateArticleRequest("Article 1", "Content 1", Guid.NewGuid());
		var article2 = new CreateArticleRequest("Article 2", "Content 2", Guid.NewGuid());
		
		await Client.PostAsJsonAsync("/api/articles", article1);
		await Client.PostAsJsonAsync("/api/articles", article2);

		// Act
		var response = await Client.GetAsync("/api/articles");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		
		var articles = await response.Content.ReadFromJsonAsync<List<ArticleResponse>>();
		articles.Should().NotBeNull().And.HaveCount(2);
	}
}
```

### Database Integration Tests

```csharp
namespace YourSolution.Tests.Integration.Features.Articles;

/// <summary>
/// Integration tests for Article repository
/// </summary>
public sealed class ArticleRepositoryTests : IntegrationTestBase
{
	private IArticleRepository GetRepository()
	{
		var scope = Factory.Services.CreateScope();
		return scope.ServiceProvider.GetRequiredService<IArticleRepository>();
	}

	[Fact]
	public async Task AddAsync_ValidArticle_SavesToDatabase()
	{
		// Arrange
		var repository = GetRepository();
		var article = new Article
		{
			Id = Guid.NewGuid(),
			Title = "Test Article",
			Content = "Test Content",
			AuthorId = Guid.NewGuid(),
			CreatedAt = DateTime.UtcNow
		};

		// Act
		await repository.AddAsync(article, CancellationToken.None);

		// Assert
		var saved = await repository.GetByIdAsync(article.Id, CancellationToken.None);
		saved.Should().NotBeNull();
		saved!.Title.Should().Be(article.Title);
		saved.Content.Should().Be(article.Content);
	}

	[Fact]
	public async Task GetByIdAsync_NonExistentArticle_ReturnsNull()
	{
		// Arrange
		var repository = GetRepository();
		var nonExistentId = Guid.NewGuid();

		// Act
		var article = await repository.GetByIdAsync(nonExistentId, CancellationToken.None);

		// Assert
		article.Should().BeNull();
	}

	[Fact]
	public async Task UpdateAsync_ExistingArticle_UpdatesDatabase()
	{
		// Arrange
		var repository = GetRepository();
		var article = new Article
		{
			Id = Guid.NewGuid(),
			Title = "Original Title",
			Content = "Original Content",
			AuthorId = Guid.NewGuid(),
			CreatedAt = DateTime.UtcNow
		};
		
		await repository.AddAsync(article, CancellationToken.None);

		// Act
		article.Title = "Updated Title";
		article.Content = "Updated Content";
		await repository.UpdateAsync(article, CancellationToken.None);

		// Assert
		var updated = await repository.GetByIdAsync(article.Id, CancellationToken.None);
		updated.Should().NotBeNull();
		updated!.Title.Should().Be("Updated Title");
		updated.Content.Should().Be("Updated Content");
	}
}
```

## Running Integration Tests

### Command Line

```bash
# Run all integration tests
dotnet test tests/YourSolution.Tests.Integration/

# Run with detailed output
dotnet test tests/YourSolution.Tests.Integration/ --logger "console;verbosity=detailed"

# Run specific test class
dotnet test --filter "FullyQualifiedName~CreateArticleEndpointTests"

# Run tests in parallel (faster but use with caution)
dotnet test tests/YourSolution.Tests.Integration/ --parallel

# Run with environment variable
TEST_DATABASE_NAME=integration_test dotnet test tests/YourSolution.Tests.Integration/
```

### Visual Studio

1. Open **Test Explorer** (Test → Test Explorer)
2. Filter tests to show only integration tests
3. Right-click and select **Run** or **Debug Selected Tests**
4. Monitor test output in the Output window

### CI/CD Pipeline

Integration tests in GitHub Actions:

```yaml
name: Integration Tests

on:
  pull_request:
  push:
    branches: [main, develop]

jobs:
  integration-tests:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Run integration tests
      run: dotnet test tests/YourSolution.Tests.Integration/ --no-build --logger trx
    
    - name: Upload test results
      if: always()
      uses: actions/upload-artifact@v4
      with:
        name: integration-test-results
        path: TestResults/**/*.trx
```

## Debugging Integration Tests

### Visual Studio

1. Set breakpoints in your test or application code
2. Right-click the test in Test Explorer
3. Select **Debug Selected Tests**
4. Step through code using F10 (Step Over) and F11 (Step Into)

### Inspecting Test Containers

To connect to a running test container for troubleshooting:

```csharp
public sealed class CreateArticleEndpointTests : IntegrationTestBase
{
	[Fact]
	public async Task DebugTest()
	{
		// Set a breakpoint here
		var connectionString = _mongoContainer!.GetConnectionString();
		
		// Use this connection string with MongoDB Compass or Studio 3T
		// to inspect the database while the test is paused
		
		// ... rest of test
	}
}
```

### Viewing HTTP Requests/Responses

Enable logging to see HTTP traffic:

```csharp
protected override void ConfigureWebHost(IWebHostBuilder builder)
{
	builder.ConfigureLogging(logging =>
	{
		logging.ClearProviders();
		logging.AddConsole();
		logging.SetMinimumLevel(LogLevel.Debug);
	});
}
```

## Best Practices

### 1. Use TestContainers for Real Dependencies

```csharp
// Good - Uses real MongoDB in container
private MongoDbContainer _mongoContainer = new MongoDbBuilder()
	.WithImage("mongo:7.0")
	.Build();

// Avoid - In-memory database might not match production behavior
```

### 2. Clean Database Between Tests

```csharp
public async Task InitializeAsync()
{
	await _mongoContainer.StartAsync();
	await CleanDatabase();
}

private async Task CleanDatabase()
{
	var client = new MongoClient(_mongoContainer.GetConnectionString());
	var database = client.GetDatabase("test-db");
	await database.DropAsync();
}
```

### 3. Test Real HTTP Responses

```csharp
// Good - Tests actual HTTP response
var response = await Client.GetAsync("/api/articles");
response.StatusCode.Should().Be(HttpStatusCode.OK);

// Avoid - Bypasses HTTP layer
var service = Factory.Services.GetRequiredService<IArticleService>();
var result = await service.GetArticlesAsync();
```

### 4. Test Error Scenarios

```csharp
[Fact]
public async Task CreateArticle_DuplicateTitle_ReturnsConflict()
{
	var request = new CreateArticleRequest("Duplicate Title", "Content", Guid.NewGuid());
	
	await Client.PostAsJsonAsync("/api/articles", request);
	var response = await Client.PostAsJsonAsync("/api/articles", request);
	
	response.StatusCode.Should().Be(HttpStatusCode.Conflict);
}
```

### 5. Use Realistic Test Data

```csharp
// Good - Realistic data
var article = new Article
{
	Title = "Understanding Microservices Architecture",
	Content = "Microservices is an architectural pattern...",
	AuthorId = Guid.NewGuid(),
	CreatedAt = DateTime.UtcNow
};

// Avoid - Minimal unrealistic data
var article = new Article { Title = "a", Content = "b" };
```

## Common Issues and Solutions

### Issue: Tests Fail Due to Port Conflicts

**Cause**: Multiple test containers trying to use the same port  
**Solution**: Use dynamic port binding

```csharp
var mongoContainer = new MongoDbBuilder()
	.WithImage("mongo:7.0")
	.WithPortBinding(27017, assignRandomHostPort: true) // Random port
	.Build();
```

### Issue: Tests Take Too Long

**Cause**: Starting containers for each test  
**Solution**: Share containers across test class

```csharp
public sealed class ArticleEndpointTests : IClassFixture<IntegrationTestFixture>
{
	private readonly IntegrationTestFixture _fixture;

	public ArticleEndpointTests(IntegrationTestFixture fixture)
	{
		_fixture = fixture;
	}
	
	// Container is created once for all tests in this class
}
```

### Issue: Flaky Tests Due to Timing

**Cause**: Async operations not properly awaited  
**Solution**: Always await async operations

```csharp
// Bad
var response = Client.PostAsJsonAsync("/api/articles", request); // Not awaited

// Good
var response = await Client.PostAsJsonAsync("/api/articles", request);
```

### Issue: Cannot Connect to Test Container

**Cause**: Container not fully started before tests run  
**Solution**: Wait for container to be ready

```csharp
await _mongoContainer.StartAsync();
await WaitForContainerReady(_mongoContainer);

private async Task WaitForContainerReady(MongoDbContainer container)
{
	var client = new MongoClient(container.GetConnectionString());
	var timeout = TimeSpan.FromSeconds(30);
	var stopwatch = Stopwatch.StartNew();
	
	while (stopwatch.Elapsed < timeout)
	{
		try
		{
			await client.ListDatabaseNamesAsync();
			return;
		}
		catch
		{
			await Task.Delay(500);
		}
	}
	
	throw new TimeoutException("Container failed to start in time");
}
```

## Performance Optimization

### Run Tests in Parallel

```csharp
[Collection("Integration Tests")] // Prevents parallel execution if needed
public sealed class CreateArticleEndpointTests : IntegrationTestBase
{
	// Tests run sequentially within this collection
}
```

### Reuse Containers

Create a shared fixture for expensive resources:

```csharp
public sealed class IntegrationTestFixture : IAsyncLifetime
{
	public MongoDbContainer MongoContainer { get; private set; } = null!;

	public async Task InitializeAsync()
	{
		MongoContainer = new MongoDbBuilder().Build();
		await MongoContainer.StartAsync();
	}

	public async Task DisposeAsync()
	{
		await MongoContainer.DisposeAsync();
	}
}
```

## Additional Resources

- [TestContainers Documentation](https://dotnet.testcontainers.org/)
- [WebApplicationFactory Documentation](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [.NET Integration Testing Best Practices](https://learn.microsoft.com/en-us/dotnet/core/testing/integration-testing)
