namespace Web.Components.Shared;

/// <summary>
/// Unit tests for ErrorPageComponent. Uses BunitContext and Render per bUnit migration guide.
/// </summary>
[ExcludeFromCodeCoverage]
public class ErrorPageComponentTests : BunitContext
{
  [Theory]
  [InlineData(401, "401 Unauthorized", "You are not authorized to view this page.")]
  [InlineData(403, "403 Forbidden", "Access to this resource is forbidden.")]
  [InlineData(404, "404 Not Found", "The page you are looking for does not exist.")]
  [InlineData(500, "500 Internal Server Error", "An unexpected error occured on the server.")]
  [InlineData(999, "Unknown Error", "An error occurred. Please try again later.")]
  public void ErrorPageComponent_Renders_Correct_Title_And_Message(int errorCode, string expectedTitle, string expectedMessageStart)
  {
    // Act
    var cut = Render<ErrorPageComponent>(parameters => parameters
      .Add(p => p.ErrorCode, errorCode)
    );

    // Assert
    cut.Markup.Should().Contain(expectedTitle);
    cut.Markup.Should().Contain(expectedMessageStart);
  }

  [Fact]
  public void ErrorPageComponent_Uses_Custom_Styles()
  {
    // Act
    var cut = Render<ErrorPageComponent>(parameters => parameters
      .Add(p => p.ErrorCode, 404)
      .Add(p => p.ShadowStyle, "shadow-blue-500")
      .Add(p => p.TextColor, "blue-600")
    );

    // Assert
    cut.Markup.Should().Contain("shadow-blue-500");
    cut.Markup.Should().Contain("blue-600");
  }

  [Fact]
  public async Task ErrorPageComponent_TriggerErrorAsync_DoesNotThrow()
  {
    // Arrange
    var cut = Render<ErrorPageComponent>(parameters => parameters.Add(p => p.ErrorCode, 500));
    var instance = cut.Instance;

    // Act & Assert
    await instance.TriggerErrorAsync(new Exception("test"));
  }
}
