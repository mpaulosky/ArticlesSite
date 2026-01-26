using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using PSC.Blazor.Components.MarkdownEditor.Models;

namespace Web.Components.Shared;

[ExcludeFromCodeCoverage]
public class TextEditorTests : BunitContext
{
	private readonly IJSRuntime _jsRuntime = Substitute.For<IJSRuntime>();
	private readonly IFileStorage _fileStorage = Substitute.For<IFileStorage>();

	public TextEditorTests()
	{
		Services.AddSingleton(_jsRuntime);
		Services.AddSingleton(_fileStorage);
	}

	[Fact]
	public void Renders_MarkdownEditor_WithToolbar()
	{
		var cut = Render<TextEditor>(parameters => parameters
			.Add(p => p.AlignmentOptionsEnabled, true)
			.Add(p => p.Content, "Initial content")
			.Add(p => p.ContentChanged, EventCallback.Factory.Create<string>(this, _ => { }))
		);
		cut.Markup.Should().Contain("<textarea");
	}

	[Fact]
	public async Task HandleImageUpload_SetsUploadUrl()
	{
		// Arrange
		var file = new FileEntry { Name = "test.png", Size = 123, ContentBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3 }), LastModified = DateTime.UtcNow };
		_fileStorage.AddFile(Arg.Any<FileData>()).Returns("testfileid");
		var nav = Services.GetRequiredService<NavigationManager>();
		var cut = Render<TextEditor>(parameters => parameters
			.Add(p => p.AlignmentOptionsEnabled, false)
			.Add(p => p.Content, "")
			.Add(p => p.ContentChanged, EventCallback.Factory.Create<string>(this, _ => { }))
		);
		var instance = cut.Instance;
		// Act
		var result = await instance.HandleImageUpload(null!, file);
		// Assert
		Assert.NotNull(result.UploadUrl);
		Assert.Contains("/api/files/testfileid", result.UploadUrl);
	}

	[Fact]
	public async Task OnCustomButtonClicked_WrapsSelectedTextWithAlignment()
	{
		// Arrange
		var selectedText = "Hello";
		_jsRuntime.InvokeAsync<string>("getSelectedText", Arg.Any<object[]>())
			.Returns(selectedText);
		var cut = Render<TextEditor>(parameters => parameters
			.Add(p => p.AlignmentOptionsEnabled, true)
			.Add(p => p.Content, "Hello world!")
			.Add(p => p.ContentChanged, EventCallback.Factory.Create<string>(this, _ => { }))
		);
		var instance = cut.Instance;
		instance.MyContent = "Hello world!";
		// Act
		var args = new PSC.Blazor.Components.MarkdownEditor.EventsArgs.MarkdownButtonEventArgs("align", "center");
		await instance.OnCustomButtonClicked(args);
		// Assert
		Assert.Contains("<div style='text-align:center'>Hello</div>", instance.MyContent);
	}
}
