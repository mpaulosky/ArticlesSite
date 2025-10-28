# Bunit Testing Guide for Web.Tests.Unit

## Quick Reference

### Basic Test Structure

```csharp
using Bunit;
using Web.Components.Shared;

namespace Web.Tests.Unit.Components.Shared;

[ExcludeFromCodeCoverage]
public class MyComponentTests : Bunit.TestContext
{
    [Fact]
    public void MyComponent_ShouldRender_WithExpectedContent()
    {
        // Act
        var cut = Render<MyComponent>();
        
        // Assert
        cut.Find("h1").TextContent.Should().Be("Expected Text");
    }
}
```

### Testing Components with Parameters

```csharp
[Fact]
public void MyComponent_WithParameters_ShouldRender()
{
    // Arrange
    var title = "Test Title";
    var count = 5;
    
    // Act
    var cut = Render<MyComponent>(parameters => parameters
        .Add(p => p.Title, title)
        .Add(p => p.Count, count));
    
    // Assert
    cut.Find("h2").TextContent.Should().Be(title);
}
```

### Testing Components with Child Content

```csharp
[Fact]
public void MyComponent_WithChildContent_ShouldRender()
{
    // Arrange
    var childText = "Child content here";
    
    // Act
    var cut = Render<MyComponent>(parameters => parameters
        .AddChildContent(childText));
    
    // Assert
    cut.Markup.Should().Contain(childText);
}
```

### Testing CSS Classes

```csharp
[Fact]
public void MyComponent_ShouldHave_CorrectStyling()
{
    // Act
    var cut = Render<MyComponent>();
    
    // Assert
    var element = cut.Find(".my-class");
    element.ClassList.Should().Contain("bg-blue-500");
    element.ClassList.Should().Contain("rounded-md");
}
```

### Testing with Services/Dependencies

```csharp
[Fact]
public void MyComponent_WithMockedService_ShouldWork()
{
    // Arrange
    var mockService = Substitute.For<IMyService>();
    mockService.GetData().Returns("Test Data");
    Services.AddSingleton(mockService);
    
    // Act
    var cut = Render<MyComponent>();
    
    // Assert
    cut.Find("div").TextContent.Should().Contain("Test Data");
}
```

### Testing Event Handlers

```csharp
[Fact]
public void MyComponent_WhenClicked_ShouldInvokeCallback()
{
    // Arrange
    var callbackInvoked = false;
    var cut = Render<MyComponent>(parameters => parameters
        .Add(p => p.OnClick, () => callbackInvoked = true));
    
    // Act
    cut.Find("button").Click();
    
    // Assert
    callbackInvoked.Should().BeTrue();
}
```

## Components That Need Bunit Tests

### High Priority (Simple Components)
- [ ] `PageHeaderComponent.razor`
- [ ] `PageHeadingComponent.razor`
- [ ] `ComponentHeadingComponent.razor`
- [ ] `ReconnectModal.razor`

### Medium Priority (Components with Logic)
- [ ] `LoginComponent.razor`
- [ ] `FooterComponent.razor`
- [ ] `NavMenuComponent.razor`
- [ ] `ConnectWithUsComponent.razor`
- [ ] `PostInfoComponent.razor`

### Lower Priority (Page Components)
- [ ] `Home.razor`
- [ ] `About.razor`
- [ ] `Contact.razor`
- [ ] `Error.razor`
- [ ] `NotFound.razor`

### Complex Components (May Need Mocking)
- [ ] `MainLayout.razor` (currently tested via reflection)
- [ ] `Profile.razor` (currently tested via reflection)
- [ ] Feature components (Details, List, Create, Edit)

## Best Practices

1. **Inherit from `Bunit.TestContext`** (not `Xunit.TestContext`)
2. **Use `Render<TComponent>()`** instead of obsolete `RenderComponent<TComponent>()`
3. **Test one thing per test** - keep tests focused and simple
4. **Use descriptive test names** - follow pattern: `ComponentName_Scenario_ExpectedResult`
5. **Arrange-Act-Assert** - keep test structure clear
6. **Mock external dependencies** - use NSubstitute for services
7. **Verify both content and behavior** - test what users see and do

## Common Assertions

```csharp
// Find elements
cut.Find("selector")
cut.FindAll("selector")

// Text content
element.TextContent.Should().Be("expected")
element.InnerHtml.Should().Contain("text")

// CSS classes
element.ClassList.Should().Contain("class-name")

// Attributes
element.GetAttribute("id").Should().Be("my-id")

// Markup
cut.Markup.Should().Contain("<div>")

// Element existence
cut.FindAll("selector").Should().HaveCount(3)
```

## Running Tests

```powershell
# Run all Bunit tests
dotnet test --filter "FullyQualifiedName~ComponentTests"

# Run specific test class
dotnet test --filter "FullyQualifiedName~LoadingComponentTests"

# Run with detailed output
dotnet test --verbosity normal
```

## Resources

- [Bunit Documentation](https://bunit.dev/)
- [Bunit GitHub](https://github.com/bUnit-dev/bUnit)
- [Migration Guide to v2](https://bunit.dev/docs/migrations)
