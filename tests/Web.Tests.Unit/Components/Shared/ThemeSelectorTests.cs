// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ThemeSelectorTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Unit
// =======================================================

using Microsoft.JSInterop;

namespace Web.Components.Shared;

/// <summary>
/// Unit tests for ThemeSelector component using Bunit.
/// Tests cover rendering, theme selection, JS interop, and error handling.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeSelectorTests : BunitContext
{
	[Fact]
	public void ThemeSelector_Renders_WithCorrectStructure()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.syncUI");

		// Act
		var cut = Render<ThemeSelector>();

		// Assert
		cut.Find(".theme-controls").Should().NotBeNull();
		cut.Find(".color-selector").Should().NotBeNull();
		cut.Find(".brightness-selector").Should().NotBeNull();
		cut.Find(".theme-info").Should().NotBeNull();
	}

	[Fact]
	public void ThemeSelector_Renders_AllColorButtons()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.syncUI");

		// Act
		var cut = Render<ThemeSelector>();

		// Assert
		cut.Find("#btn-red").TextContent.Should().Contain("Red");
		cut.Find("#btn-blue").TextContent.Should().Contain("Blue");
		cut.Find("#btn-green").TextContent.Should().Contain("Green");
		cut.Find("#btn-yellow").TextContent.Should().Contain("Yellow");
	}

	[Fact]
	public void ThemeSelector_Renders_AllBrightnessButtons()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.syncUI");

		// Act
		var cut = Render<ThemeSelector>();

		// Assert
		cut.Find("#btn-light").TextContent.Should().Contain("Light(Pastel)");
		cut.Find("#btn-dark").TextContent.Should().Contain("Dark(Rich)");
	}

	[Fact]
	public void ThemeSelector_Renders_Labels()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.syncUI");

		// Act
		var cut = Render<ThemeSelector>();

		// Assert
		var labels = cut.FindAll(".theme-selector-label");
		labels.Should().HaveCount(2);
		labels[0].TextContent.Should().Contain("Color:");
		labels[1].TextContent.Should().Contain("Brightness:");
	}

	[Fact]
	public void ThemeSelector_Renders_CurrentThemeInfo()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.syncUI");

		// Act
		var cut = Render<ThemeSelector>();

		// Assert
		var currentTheme = cut.Find("#current-theme");
		currentTheme.Should().NotBeNull();
		currentTheme.TextContent.Should().Be("Blue Light");
	}

	[Fact]
	public async Task ThemeSelector_OnAfterRenderAsync_CallsSyncUI_OnFirstRender()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.syncUI");

		// Act
		var cut = Render<ThemeSelector>();
		await cut.InvokeAsync(() => Task.CompletedTask);

		// Assert
		var invocations = JSInterop.Invocations["ThemeManager.syncUI"];
		invocations.Should().HaveCount(1, "ThemeManager.syncUI should be called once on first render");
	}

	[Fact]
	public async Task ThemeSelector_SelectColor_RED_CallsJSInterop()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.syncUI");
		JSInterop.SetupVoid("ThemeManager.selectColorAndUpdateUI", _ => true);

		var cut = Render<ThemeSelector>();

		// Act
		var redButton = cut.Find("#btn-red");
		await redButton.ClickAsync(new());

		// Assert
		JSInterop.VerifyInvoke("ThemeManager.selectColorAndUpdateUI");
		var invocations = JSInterop.Invocations["ThemeManager.selectColorAndUpdateUI"];
		invocations.Should().HaveCount(1);
		invocations[0].Arguments[0].Should().Be("RED");
	}

	[Fact]
	public async Task ThemeSelector_SelectColor_BLUE_CallsJSInterop()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.syncUI");
		JSInterop.SetupVoid("ThemeManager.selectColorAndUpdateUI", _ => true);

		var cut = Render<ThemeSelector>();

		// Act
		var blueButton = cut.Find("#btn-blue");
		await blueButton.ClickAsync(new());

		// Assert
		JSInterop.VerifyInvoke("ThemeManager.selectColorAndUpdateUI");
		var invocations = JSInterop.Invocations["ThemeManager.selectColorAndUpdateUI"];
		invocations.Should().HaveCount(1);
		invocations[0].Arguments[0].Should().Be("BLUE");
	}

	[Fact]
	public async Task ThemeSelector_SelectColor_GREEN_CallsJSInterop()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.syncUI");
		JSInterop.SetupVoid("ThemeManager.selectColorAndUpdateUI", _ => true);

		var cut = Render<ThemeSelector>();

		// Act
		var greenButton = cut.Find("#btn-green");
		await greenButton.ClickAsync(new());

		// Assert
		JSInterop.VerifyInvoke("ThemeManager.selectColorAndUpdateUI");
		var invocations = JSInterop.Invocations["ThemeManager.selectColorAndUpdateUI"];
		invocations.Should().HaveCount(1);
		invocations[0].Arguments[0].Should().Be("GREEN");
	}

	[Fact]
	public async Task ThemeSelector_SelectColor_YELLOW_CallsJSInterop()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.syncUI");
		JSInterop.SetupVoid("ThemeManager.selectColorAndUpdateUI", _ => true);

		var cut = Render<ThemeSelector>();

		// Act
		var yellowButton = cut.Find("#btn-yellow");
		await yellowButton.ClickAsync(new());

		// Assert
		JSInterop.VerifyInvoke("ThemeManager.selectColorAndUpdateUI");
		var invocations = JSInterop.Invocations["ThemeManager.selectColorAndUpdateUI"];
		invocations.Should().HaveCount(1);
		invocations[0].Arguments[0].Should().Be("YELLOW");
	}

	[Fact]
	public async Task ThemeSelector_SelectBrightness_Light_CallsJSInterop()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.syncUI");
		JSInterop.SetupVoid("ThemeManager.selectBrightnessAndUpdateUI", _ => true);

		var cut = Render<ThemeSelector>();

		// Act
		var lightButton = cut.Find("#btn-light");
		await lightButton.ClickAsync(new());

		// Assert
		JSInterop.VerifyInvoke("ThemeManager.selectBrightnessAndUpdateUI");
		var invocations = JSInterop.Invocations["ThemeManager.selectBrightnessAndUpdateUI"];
		invocations.Should().HaveCount(1);
		invocations[0].Arguments[0].Should().Be("light");
	}

	[Fact]
	public async Task ThemeSelector_SelectBrightness_Dark_CallsJSInterop()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.syncUI");
		JSInterop.SetupVoid("ThemeManager.selectBrightnessAndUpdateUI", _ => true);

		var cut = Render<ThemeSelector>();

		// Act
		var darkButton = cut.Find("#btn-dark");
		await darkButton.ClickAsync(new());

		// Assert
		JSInterop.VerifyInvoke("ThemeManager.selectBrightnessAndUpdateUI");
		var invocations = JSInterop.Invocations["ThemeManager.selectBrightnessAndUpdateUI"];
		invocations.Should().HaveCount(1);
		invocations[0].Arguments[0].Should().Be("dark");
	}

	[Fact]
	public void ThemeSelector_ColorButtons_HaveCorrectCssClasses()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.syncUI");

		// Act
		var cut = Render<ThemeSelector>();

		// Assert
		var colorButtons = cut.FindAll(".color-btn");
		colorButtons.Should().HaveCount(4);

		// Blue button should have active class by default
		var blueButton = cut.Find("#btn-blue");
		blueButton.ClassList.Should().Contain("active");
	}

	[Fact]
	public void ThemeSelector_BrightnessButtons_HaveCorrectCssClasses()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.syncUI");

		// Act
		var cut = Render<ThemeSelector>();

		// Assert
		var brightnessButtons = cut.FindAll(".brightness-btn");
		brightnessButtons.Should().HaveCount(2);

		// Light button should have active class by default
		var lightButton = cut.Find("#btn-light");
		lightButton.ClassList.Should().Contain("active");
	}

	[Fact]
	public void ThemeSelector_HasButtonGroups()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.syncUI");

		// Act
		var cut = Render<ThemeSelector>();

		// Assert
		var buttonGroups = cut.FindAll(".button-group");
		buttonGroups.Should().HaveCount(2, "should have one for colors and one for brightness");
	}

	[Fact]
	public void ThemeSelector_ColorSelector_ContainsAllColorButtons()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.syncUI");

		// Act
		var cut = Render<ThemeSelector>();

		// Assert
		var colorSelector = cut.Find(".color-selector");
		var colorButtons = colorSelector.QuerySelectorAll(".color-btn");
		colorButtons.Should().HaveCount(4);
	}

	[Fact]
	public void ThemeSelector_BrightnessSelector_ContainsAllBrightnessButtons()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.syncUI");

		// Act
		var cut = Render<ThemeSelector>();

		// Assert
		var brightnessSelector = cut.Find(".brightness-selector");
		var brightnessButtons = brightnessSelector.QuerySelectorAll(".brightness-btn");
		brightnessButtons.Should().HaveCount(2);
	}
}
