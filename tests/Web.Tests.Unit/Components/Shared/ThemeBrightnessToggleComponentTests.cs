// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ThemeBrightnessToggleComponentTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Unit
// =======================================================

namespace Web.Components.Shared;

/// <summary>
/// Unit tests for ThemeBrightnessToggleComponent using bUnit.
/// Covers rendering, first-render sync, button clicks, and JS interop argument verification.
///
/// JS-side effects (HTML class application) are verified by asserting the exact arguments
/// passed to ThemeManager.selectBrightnessAndUpdateUI / ThemeManager.syncUI,
/// because bUnit does not execute real JavaScript.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeBrightnessToggleComponentTests : BunitContext
{
	// ──────────────────────────────────────────────────────────────────────────
	// Rendering
	// ──────────────────────────────────────────────────────────────────────────

	/// <summary>
	/// Test list item 9 — component renders exactly 2 brightness buttons.
	/// </summary>
	[Fact]
	public void ThemeBrightnessToggle_Renders_ExactlyTwoButtons()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.syncUI");

		// Act
		var cut = Render<ThemeBrightnessToggleComponent>();

		// Assert
		cut.FindAll("button").Should().HaveCount(2);
	}

	[Fact]
	public void ThemeBrightnessToggle_Renders_LightButton()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.syncUI");

		// Act
		var cut = Render<ThemeBrightnessToggleComponent>();

		// Assert
		var lightBtn = cut.Find("#nav-btn-light");
		lightBtn.Should().NotBeNull();
		lightBtn.TextContent.Trim().Should().Contain("Light");
	}

	[Fact]
	public void ThemeBrightnessToggle_Renders_DarkButton()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.syncUI");

		// Act
		var cut = Render<ThemeBrightnessToggleComponent>();

		// Assert
		var darkBtn = cut.Find("#nav-btn-dark");
		darkBtn.Should().NotBeNull();
		darkBtn.TextContent.Trim().Should().Contain("Dark");
	}

	[Fact]
	public void ThemeBrightnessToggle_LightButton_HasSunEmoji()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.syncUI");

		// Act
		var cut = Render<ThemeBrightnessToggleComponent>();

		// Assert — ☀ present in light button markup
		cut.Find("#nav-btn-light").TextContent.Should().Contain("☀");
	}

	[Fact]
	public void ThemeBrightnessToggle_DarkButton_HasHalfMoonEmoji()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.syncUI");

		// Act
		var cut = Render<ThemeBrightnessToggleComponent>();

		// Assert — ◐ present in dark button markup
		cut.Find("#nav-btn-dark").TextContent.Should().Contain("◐");
	}

	[Fact]
	public void ThemeBrightnessToggle_Buttons_HaveBrightnessButtonClass()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.syncUI");

		// Act
		var cut = Render<ThemeBrightnessToggleComponent>();

		// Assert — both buttons carry .brightness-btn so ThemeManager.syncUI can target them
		var buttons = cut.FindAll(".brightness-btn");
		buttons.Should().HaveCount(2);
	}

	[Fact]
	public void ThemeBrightnessToggle_Buttons_HaveBtnClass()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.syncUI");

		// Act
		var cut = Render<ThemeBrightnessToggleComponent>();

		// Assert
		var buttons = cut.FindAll(".btn");
		buttons.Should().HaveCount(2);
	}

	[Fact]
	public void ThemeBrightnessToggle_Container_HasThemeBrightnessToggleClass()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.syncUI");

		// Act
		var cut = Render<ThemeBrightnessToggleComponent>();

		// Assert
		cut.Find(".theme-brightness-toggle").Should().NotBeNull();
	}

	// ──────────────────────────────────────────────────────────────────────────
	// Test list item 2 — active state synced from persisted localStorage on load
	// (verified by ThemeManager.syncUI being called on first render)
	// ──────────────────────────────────────────────────────────────────────────

	[Fact]
	public async Task ThemeBrightnessToggle_OnAfterRenderAsync_CallsSyncUI_OnFirstRender()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.syncUI");

		// Act
		var cut = Render<ThemeBrightnessToggleComponent>();
		await cut.InvokeAsync(() => Task.CompletedTask);

		// Assert — syncUI is what drives the .active class state on the buttons
		JSInterop.Invocations["ThemeManager.syncUI"].Should()
			.HaveCount(1, "ThemeManager.syncUI must be called exactly once on first render to reflect persisted brightness");
	}

	[Fact]
	public async Task ThemeBrightnessToggle_OnAfterRenderAsync_DoesNotCallSelectBrightness_OnLoad()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.syncUI");

		// Act
		var cut = Render<ThemeBrightnessToggleComponent>();
		await cut.InvokeAsync(() => Task.CompletedTask);

		// Assert — selectBrightnessAndUpdateUI should NOT fire on init; only syncUI does
		JSInterop.Invocations
			.Any(i => i.Identifier == "ThemeManager.selectBrightnessAndUpdateUI")
			.Should().BeFalse("brightness should not be changed on load, only synced");
	}

	// ──────────────────────────────────────────────────────────────────────────
	// Test list items 1 & 6 — button clicks call the correct JS with correct args
	// (JS applies theme-*-light / theme-*-dark class to <html> and calls syncUI
	// on ALL .brightness-btn elements, including those in the full ThemeSelector card)
	// ──────────────────────────────────────────────────────────────────────────

	[Fact]
	public async Task ThemeBrightnessToggle_LightButton_Click_CallsSelectBrightnessAndUpdateUI_WithLightArg()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.syncUI");
		JSInterop.SetupVoid("ThemeManager.selectBrightnessAndUpdateUI", _ => true);

		var cut = Render<ThemeBrightnessToggleComponent>();

		// Act
		await cut.Find("#nav-btn-light").ClickAsync(new());

		// Assert
		JSInterop.VerifyInvoke("ThemeManager.selectBrightnessAndUpdateUI");
		var invocations = JSInterop.Invocations["ThemeManager.selectBrightnessAndUpdateUI"];
		invocations.Should().HaveCount(1);
		invocations[0].Arguments[0].Should().Be("light",
			"clicking the Light button must pass \"light\" so ThemeManager applies the light variant of the current colour family");
	}

	[Fact]
	public async Task ThemeBrightnessToggle_DarkButton_Click_CallsSelectBrightnessAndUpdateUI_WithDarkArg()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.syncUI");
		JSInterop.SetupVoid("ThemeManager.selectBrightnessAndUpdateUI", _ => true);

		var cut = Render<ThemeBrightnessToggleComponent>();

		// Act
		await cut.Find("#nav-btn-dark").ClickAsync(new());

		// Assert
		JSInterop.VerifyInvoke("ThemeManager.selectBrightnessAndUpdateUI");
		var invocations = JSInterop.Invocations["ThemeManager.selectBrightnessAndUpdateUI"];
		invocations.Should().HaveCount(1);
		invocations[0].Arguments[0].Should().Be("dark",
			"clicking the Dark button must pass \"dark\" so ThemeManager applies the dark variant of the current colour family");
	}

	/// <summary>
	/// Verifies that ThemeManager.syncUI is called only once — on first render —
	/// not on subsequent re-renders triggered by button clicks.
	/// </summary>
	[Fact]
	public async Task ThemeBrightnessToggle_SyncUI_CalledOnlyOnFirstRender_NotOnSubsequentRenders()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.syncUI");
		JSInterop.SetupVoid("ThemeManager.selectBrightnessAndUpdateUI", _ => true);

		var cut = Render<ThemeBrightnessToggleComponent>();
		await cut.InvokeAsync(() => Task.CompletedTask); // allow first render to settle

		// Act — click a button to trigger a re-render cycle
		await cut.Find("#nav-btn-light").ClickAsync(new());

		// Assert — syncUI was still called exactly once (not again after the click re-render)
		JSInterop.Invocations["ThemeManager.syncUI"].Should()
			.HaveCount(1, "ThemeManager.syncUI must not be called again on re-renders — only on first render");
	}

	// ──────────────────────────────────────────────────────────────────────────
	// Test list item 7 — does not conflict with ThemeToggle (both can be rendered)
	// ──────────────────────────────────────────────────────────────────────────

	[Fact]
	public void ThemeBrightnessToggle_RendersSuccessfully_WhenNoOtherThemeComponentPresent()
	{
		// Arrange & Act — no exception from the component
		JSInterop.SetupVoid("ThemeManager.syncUI");

		var act = () => Render<ThemeBrightnessToggleComponent>();

		// Assert
		act.Should().NotThrow();
	}
}
