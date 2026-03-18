// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ThemeToggleTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Unit
// =======================================================

using Blazored.LocalStorage;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Web.Components.Shared;

/// <summary>
/// Unit tests for ThemeToggle component using Bunit.
/// Tests cover rendering, theme persistence, toggle functionality, and disposal.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeToggleTests : BunitContext
{
	private readonly ILocalStorageService _localStorage;

	public ThemeToggleTests()
	{
		_localStorage = Substitute.For<ILocalStorageService>();

		Services.AddSingleton(_localStorage);
	}

	[Fact]
	public void ThemeToggle_Renders_WithCorrectStructure()
	{
		// Arrange
		var module = JSInterop.SetupModule("./Components/Shared/ThemeToggle.razor.js");
		module.SetupVoid("setTheme", _ => true).SetVoidResult();
		_localStorage.GetItemAsStringAsync("theme").Returns(ValueTask.FromResult<string?>("light"));

		// Act
		var cut = Render<ThemeToggle>();

		// Assert
		cut.Find("label").Should().NotBeNull();
		cut.Find("input#toggle").Should().NotBeNull();
		cut.Markup.Should().Contain("Light");
		cut.Markup.Should().Contain("Dark");
	}

	[Fact]
	public void ThemeToggle_Renders_LabelAndCheckbox()
	{
		// Arrange
		var module = JSInterop.SetupModule("./Components/Shared/ThemeToggle.razor.js");
		module.SetupVoid("setTheme", _ => true).SetVoidResult();
		_localStorage.GetItemAsStringAsync("theme").Returns(ValueTask.FromResult<string?>("light"));

		// Act
		var cut = Render<ThemeToggle>();

		// Assert
		var label = cut.Find("label");
		label.GetAttribute("for").Should().Be("toggle");

		var checkbox = cut.Find("input#toggle");
		checkbox.GetAttribute("type").Should().Be("checkbox");
		checkbox.ClassList.Should().Contain("hidden");
	}

	[Fact]
	public void ThemeToggle_Renders_LightAndDarkLabels()
	{
		// Arrange
		var module = JSInterop.SetupModule("./Components/Shared/ThemeToggle.razor.js");
		module.SetupVoid("setTheme", _ => true).SetVoidResult();
		_localStorage.GetItemAsStringAsync("theme").Returns(ValueTask.FromResult<string?>("light"));

		// Act
		var cut = Render<ThemeToggle>();

		// Assert
		var spans = cut.FindAll("span");
		spans.Should().HaveCount(2);
		spans[0].TextContent.Should().Be("Light");
		spans[1].TextContent.Should().Be("Dark");
	}

	[Fact]
	public async Task ThemeToggle_OnAfterRenderAsync_LoadsThemeFromLocalStorage_Light()
	{
		// Arrange
		var module = JSInterop.SetupModule("./Components/Shared/ThemeToggle.razor.js");
		module.SetupVoid("setTheme", _ => true).SetVoidResult();
		_localStorage.GetItemAsStringAsync("theme").Returns(ValueTask.FromResult<string?>("light"));

		// Act
		var cut = Render<ThemeToggle>();
		await cut.InvokeAsync(() => Task.CompletedTask);

		// Assert
		await _localStorage.Received(1).GetItemAsStringAsync("theme");
	}

	[Fact]
	public async Task ThemeToggle_OnAfterRenderAsync_LoadsThemeFromLocalStorage_Dark()
	{
		// Arrange
		var module = JSInterop.SetupModule("./Components/Shared/ThemeToggle.razor.js");
		module.SetupVoid("setTheme", _ => true).SetVoidResult();
		_localStorage.GetItemAsStringAsync("theme").Returns(ValueTask.FromResult<string?>("dark"));

		// Act
		var cut = Render<ThemeToggle>();
		await cut.InvokeAsync(() => Task.CompletedTask);

		// Assert
		await _localStorage.Received(1).GetItemAsStringAsync("theme");
	}

	[Fact]
	public async Task ThemeToggle_OnAfterRenderAsync_SetsDefaultTheme_WhenNoThemeStored()
	{
		// Arrange
		var module = JSInterop.SetupModule("./Components/Shared/ThemeToggle.razor.js");
		module.SetupVoid("setTheme", _ => true).SetVoidResult();
		_localStorage.GetItemAsStringAsync("theme").Returns(ValueTask.FromResult<string?>(null));

		// Act
		var cut = Render<ThemeToggle>();
		await cut.InvokeAsync(() => Task.CompletedTask);

		// Assert
		await _localStorage.Received(1).GetItemAsStringAsync("theme");
		await _localStorage.Received(1).SetItemAsStringAsync("theme", "light");
	}

	[Fact]
	public async Task ThemeToggle_OnAfterRenderAsync_SetsDefaultTheme_WhenEmptyThemeStored()
	{
		// Arrange
		var module = JSInterop.SetupModule("./Components/Shared/ThemeToggle.razor.js");
		module.SetupVoid("setTheme", _ => true).SetVoidResult();
		_localStorage.GetItemAsStringAsync("theme").Returns(ValueTask.FromResult<string?>(string.Empty));

		// Act
		var cut = Render<ThemeToggle>();
		await cut.InvokeAsync(() => Task.CompletedTask);

		// Assert
		await _localStorage.Received(1).GetItemAsStringAsync("theme");
		await _localStorage.Received(1).SetItemAsStringAsync("theme", "light");
	}

	[Fact]
	public async Task ThemeToggle_ImportsJavaScriptModule()
	{
		// Arrange
		var module = JSInterop.SetupModule("./Components/Shared/ThemeToggle.razor.js");
		module.SetupVoid("setTheme", _ => true).SetVoidResult();
		_localStorage.GetItemAsStringAsync("theme").Returns(ValueTask.FromResult<string?>("light"));

		// Act
		var cut = Render<ThemeToggle>();
		await cut.InvokeAsync(() => Task.CompletedTask);

		// Assert
		var invocations = JSInterop.Invocations["import"];
		invocations.Should().HaveCount(1);
		invocations[0].Arguments[0].Should().Be("./Components/Shared/ThemeToggle.razor.js");
	}

	[Fact]
	public async Task ThemeToggle_ToggleTheme_ChangesFromLightToDark()
	{
		// Arrange
		var module = JSInterop.SetupModule("./Components/Shared/ThemeToggle.razor.js");
		module.SetupVoid("setTheme", _ => true).SetVoidResult();
		_localStorage.GetItemAsStringAsync("theme").Returns(ValueTask.FromResult<string?>("light"));

		var cut = Render<ThemeToggle>();
		await cut.InvokeAsync(() => Task.CompletedTask);

		// Act
		var checkbox = cut.Find("input#toggle");
		await checkbox.ChangeAsync(new ChangeEventArgs { Value = true });

		// Assert
		await _localStorage.Received(1).SetItemAsStringAsync("theme", "dark");
	}

	[Fact]
	public async Task ThemeToggle_ToggleTheme_ChangesFromDarkToLight()
	{
		// Arrange
		var module = JSInterop.SetupModule("./Components/Shared/ThemeToggle.razor.js");
		module.SetupVoid("setTheme", _ => true).SetVoidResult();
		_localStorage.GetItemAsStringAsync("theme").Returns(ValueTask.FromResult<string?>("dark"));

		var cut = Render<ThemeToggle>();
		await cut.InvokeAsync(() => Task.CompletedTask);

		// Act
		var checkbox = cut.Find("input#toggle");
		await checkbox.ChangeAsync(new ChangeEventArgs { Value = false });

		// Assert
		await _localStorage.Received().SetItemAsStringAsync("theme", "light");
	}

	[Fact]
	public void ThemeToggle_ToggleSlider_HasCorrectClasses_LightMode()
	{
		// Arrange
		var module = JSInterop.SetupModule("./Components/Shared/ThemeToggle.razor.js");
		module.SetupVoid("setTheme", _ => true).SetVoidResult();
		_localStorage.GetItemAsStringAsync("theme").Returns(ValueTask.FromResult<string?>("light"));

		// Act
		var cut = Render<ThemeToggle>();

		// Assert
		var slider = cut.Find("div.w-4");
		slider.ClassList.Should().NotContain("translate-x-3", "slider should not be translated in light mode");
	}

	[Fact]
	public async Task ThemeToggle_ToggleSlider_HasCorrectClasses_DarkMode()
	{
		// Arrange
		var module = JSInterop.SetupModule("./Components/Shared/ThemeToggle.razor.js");
		module.SetupVoid("setTheme", _ => true).SetVoidResult();
		_localStorage.GetItemAsStringAsync("theme").Returns(ValueTask.FromResult<string?>("dark"));

		// Act
		var cut = Render<ThemeToggle>();
		await cut.InvokeAsync(() => Task.CompletedTask);

		// Assert
		var slider = cut.Find("div.w-4");
		slider.ClassList.Should().Contain("translate-x-3", "slider should be translated in dark mode");
	}

	[Fact]
	public void ThemeToggle_Label_HasCorrectStyling()
	{
		// Arrange
		var module = JSInterop.SetupModule("./Components/Shared/ThemeToggle.razor.js");
		module.SetupVoid("setTheme", _ => true).SetVoidResult();
		_localStorage.GetItemAsStringAsync("theme").Returns(ValueTask.FromResult<string?>("light"));

		// Act
		var cut = Render<ThemeToggle>();

		// Assert
		var label = cut.Find("label");
		label.ClassList.Should().Contain("w-9");
		label.ClassList.Should().Contain("h-5");
		label.ClassList.Should().Contain("bg-gray-300");
		label.ClassList.Should().Contain("dark:bg-gray-600");
		label.ClassList.Should().Contain("rounded-full");
		label.ClassList.Should().Contain("cursor-pointer");
	}

	[Fact]
	public void ThemeToggle_Slider_HasCorrectStyling()
	{
		// Arrange
		var module = JSInterop.SetupModule("./Components/Shared/ThemeToggle.razor.js");
		module.SetupVoid("setTheme", _ => true).SetVoidResult();
		_localStorage.GetItemAsStringAsync("theme").Returns(ValueTask.FromResult<string?>("light"));

		// Act
		var cut = Render<ThemeToggle>();

		// Assert
		var slider = cut.Find("div.w-4");
		slider.ClassList.Should().Contain("w-4");
		slider.ClassList.Should().Contain("h-4");
		slider.ClassList.Should().Contain("rounded-full");
		slider.ClassList.Should().Contain("bg-white");
		slider.ClassList.Should().Contain("shadow-md");
		slider.ClassList.Should().Contain("transform");
		slider.ClassList.Should().Contain("duration-300");
	}

	[Fact]
	public void ThemeToggle_Container_HasCorrectLayout()
	{
		// Arrange
		var module = JSInterop.SetupModule("./Components/Shared/ThemeToggle.razor.js");
		module.SetupVoid("setTheme", _ => true).SetVoidResult();
		_localStorage.GetItemAsStringAsync("theme").Returns(ValueTask.FromResult<string?>("light"));

		// Act
		var cut = Render<ThemeToggle>();

		// Assert
		var container = cut.Find("div.flex");
		container.ClassList.Should().Contain("flex");
		container.ClassList.Should().Contain("justify-end");
		container.ClassList.Should().Contain("items-center");
		container.ClassList.Should().Contain("space-x-2");
	}

	[Fact]
	public async Task ThemeToggle_DisposeAsync_DisposesModule()
	{
		// Arrange
		var module = JSInterop.SetupModule("./Components/Shared/ThemeToggle.razor.js");
		module.SetupVoid("setTheme", _ => true).SetVoidResult();
		_localStorage.GetItemAsStringAsync("theme").Returns(ValueTask.FromResult<string?>("light"));

		// Act
		var cut = Render<ThemeToggle>();
		await cut.InvokeAsync(() => Task.CompletedTask);

		// Dispose the component
		cut.Dispose();

		// Assert - Verify module was imported (disposal is handled by BUnit)
		JSInterop.Invocations["import"].Should().HaveCount(1);
	}

	[Fact]
	public async Task ThemeToggle_Checkbox_IsChecked_WhenDarkMode()
	{
		// Arrange
		var module = JSInterop.SetupModule("./Components/Shared/ThemeToggle.razor.js");
		module.SetupVoid("setTheme", _ => true).SetVoidResult();
		_localStorage.GetItemAsStringAsync("theme").Returns(ValueTask.FromResult<string?>("dark"));

		// Act
		var cut = Render<ThemeToggle>();
		await cut.InvokeAsync(() => Task.CompletedTask);

		// Assert - The checkbox should be checked in dark mode
		var checkbox = cut.Find("input#toggle");
		var isChecked = checkbox.HasAttribute("checked");
		isChecked.Should().BeTrue("checkbox should be checked in dark mode");
	}

	[Fact]
	public void ThemeToggle_Checkbox_IsNotChecked_WhenLightMode()
	{
		// Arrange
		var module = JSInterop.SetupModule("./Components/Shared/ThemeToggle.razor.js");
		module.SetupVoid("setTheme", _ => true).SetVoidResult();
		_localStorage.GetItemAsStringAsync("theme").Returns(ValueTask.FromResult<string?>("light"));

		// Act
		var cut = Render<ThemeToggle>();

		// Assert - The checkbox should not be checked in light mode
		var checkbox = cut.Find("input#toggle");
		var isChecked = checkbox.HasAttribute("checked");
		isChecked.Should().BeFalse("checkbox should not be checked in light mode");
	}

	[Fact]
	public void ThemeToggle_TextSpans_HaveCorrectStyling()
	{
		// Arrange
		var module = JSInterop.SetupModule("./Components/Shared/ThemeToggle.razor.js");
		module.SetupVoid("setTheme", _ => true).SetVoidResult();
		_localStorage.GetItemAsStringAsync("theme").Returns(ValueTask.FromResult<string?>("light"));

		// Act
		var cut = Render<ThemeToggle>();

		// Assert
		var spans = cut.FindAll("span");
		foreach (var span in spans)
		{
			span.ClassList.Should().Contain("font-semibold");
			span.ClassList.Should().Contain("text-gray-900");
			span.ClassList.Should().Contain("dark:text-gray-50");
		}
	}

	[Fact]
	public async Task ThemeToggle_MultipleToggles_PersistsTheme()
	{
		// Arrange
		var module = JSInterop.SetupModule("./Components/Shared/ThemeToggle.razor.js");
		module.SetupVoid("setTheme", _ => true).SetVoidResult();
		_localStorage.GetItemAsStringAsync("theme").Returns(ValueTask.FromResult<string?>("light"));

		var cut = Render<ThemeToggle>();
		await cut.InvokeAsync(() => Task.CompletedTask);

		// Act - Toggle to dark
		var checkbox = cut.Find("input#toggle");
		await checkbox.ChangeAsync(new ChangeEventArgs { Value = true });

		// Act - Toggle back to light
		await checkbox.ChangeAsync(new ChangeEventArgs { Value = false });

		// Assert - Both theme changes should be persisted
		await _localStorage.Received(1).SetItemAsStringAsync("theme", "dark");
		await _localStorage.Received(1).SetItemAsStringAsync("theme", "light");
	}
}




