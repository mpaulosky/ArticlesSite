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
///
/// The component is a single toggle button. _currentBrightness defaults to
/// "light" (shows yellow sun ☀). OnAfterRenderAsync(true) calls ThemeManager.syncUI
/// only. ToggleBrightnessAsync computes the opposite brightness, calls
/// ThemeManager.selectBrightnessAndUpdateUI, updates the field, and calls
/// StateHasChanged so the icon refreshes immediately.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeBrightnessToggleComponentTests : BunitContext
{
	/// <summary>
	/// Switches JSInterop to Loose mode so selectBrightnessAndUpdateUI can be
	/// called any number of times without additional SetupVoid registrations.
	/// syncUI is still explicitly set up so its invocation count can be verified.
	/// Invocations are tracked in both Strict and Loose modes.
	/// </summary>
	private void SetupJsInteropForClick()
	{
		JSInterop.Mode = JSRuntimeMode.Loose;
		JSInterop.SetupVoid("ThemeManager.syncUI");
	}

	// ──────────────────────────────────────────────────────────────────────────
	// Rendering
	// ──────────────────────────────────────────────────────────────────────────

	[Fact]
	public void ThemeBrightnessToggle_Renders_ExactlyOneButton()
	{
		JSInterop.SetupVoid("ThemeManager.syncUI");
		var cut = Render<ThemeBrightnessToggleComponent>();
		cut.FindAll("button").Should().HaveCount(1);
	}

	[Fact]
	public void ThemeBrightnessToggle_Button_HasBrightnessToggleId()
	{
		JSInterop.SetupVoid("ThemeManager.syncUI");
		var cut = Render<ThemeBrightnessToggleComponent>();
		cut.Find("#nav-btn-brightness-toggle").Should().NotBeNull();
	}

	[Fact]
	public void ThemeBrightnessToggle_Button_HasBrightnessButtonClass()
	{
		JSInterop.SetupVoid("ThemeManager.syncUI");
		var cut = Render<ThemeBrightnessToggleComponent>();
		cut.FindAll(".brightness-btn").Should().HaveCount(1);
	}

	[Fact]
	public void ThemeBrightnessToggle_Button_HasBtnClass()
	{
		JSInterop.SetupVoid("ThemeManager.syncUI");
		var cut = Render<ThemeBrightnessToggleComponent>();
		cut.FindAll(".btn").Should().HaveCount(1);
	}

	/// <summary>Default state is light — button shows yellow sun ☀.</summary>
	[Fact]
	public void ThemeBrightnessToggle_Button_ShowsSunIconByDefault()
	{
		JSInterop.SetupVoid("ThemeManager.syncUI");
		var cut = Render<ThemeBrightnessToggleComponent>();
		cut.Find("#nav-btn-brightness-toggle").TextContent.Should().Contain("☀");
	}

	/// <summary>After clicking from light, button shows dark half-circle ◐.</summary>
	[Fact]
	public async Task ThemeBrightnessToggle_Button_ShowsDarkIconAfterToggleFromLight()
	{
		SetupJsInteropForClick();
		var cut = Render<ThemeBrightnessToggleComponent>();
		await cut.Find("#nav-btn-brightness-toggle").ClickAsync(new());
		cut.Find("#nav-btn-brightness-toggle").TextContent.Should().Contain("◐");
	}

	/// <summary>After clicking twice, button returns to sun ☀.</summary>
	[Fact]
	public async Task ThemeBrightnessToggle_Button_ShowsSunIconAfterDoubleToggle()
	{
		SetupJsInteropForClick();
		var cut = Render<ThemeBrightnessToggleComponent>();
		await cut.Find("#nav-btn-brightness-toggle").ClickAsync(new());
		await cut.Find("#nav-btn-brightness-toggle").ClickAsync(new());
		cut.Find("#nav-btn-brightness-toggle").TextContent.Should().Contain("☀");
	}

	// ──────────────────────────────────────────────────────────────────────────
	// First-render sync
	// ──────────────────────────────────────────────────────────────────────────

	[Fact]
	public async Task ThemeBrightnessToggle_OnAfterRenderAsync_CallsSyncUI_OnFirstRender()
	{
		JSInterop.SetupVoid("ThemeManager.syncUI");
		var cut = Render<ThemeBrightnessToggleComponent>();
		await cut.InvokeAsync(() => Task.CompletedTask);
		JSInterop.Invocations["ThemeManager.syncUI"].Should().HaveCount(1, "syncUI called once on first render");
	}

	[Fact]
	public async Task ThemeBrightnessToggle_OnAfterRenderAsync_DoesNotCallSelectBrightness_OnLoad()
	{
		JSInterop.SetupVoid("ThemeManager.syncUI");
		var cut = Render<ThemeBrightnessToggleComponent>();
		await cut.InvokeAsync(() => Task.CompletedTask);
		JSInterop.Invocations.Any(i => i.Identifier == "ThemeManager.selectBrightnessAndUpdateUI")
			.Should().BeFalse("selectBrightnessAndUpdateUI must not fire on init");
	}

	/// <summary>syncUI must only be called on first render, not on re-renders after a click.</summary>
	[Fact]
	public async Task ThemeBrightnessToggle_SyncUI_CalledOnlyOnFirstRender_NotOnSubsequentRenders()
	{
		SetupJsInteropForClick();
		var cut = Render<ThemeBrightnessToggleComponent>();
		await cut.InvokeAsync(() => Task.CompletedTask);
		await cut.Find("#nav-btn-brightness-toggle").ClickAsync(new());
		JSInterop.Invocations["ThemeManager.syncUI"].Should().HaveCount(1, "syncUI not called again on re-render");
	}

	// ──────────────────────────────────────────────────────────────────────────
	// Toggle — JS argument assertions
	// ──────────────────────────────────────────────────────────────────────────

	[Fact]
	public async Task ThemeBrightnessToggle_Toggle_WhenCurrentIsLight_CallsSelectBrightnessWithDark()
	{
		SetupJsInteropForClick();
		var cut = Render<ThemeBrightnessToggleComponent>();
		await cut.Find("#nav-btn-brightness-toggle").ClickAsync(new());
		JSInterop.VerifyInvoke("ThemeManager.selectBrightnessAndUpdateUI");
		var invocations = JSInterop.Invocations["ThemeManager.selectBrightnessAndUpdateUI"];
		invocations.Should().HaveCount(1);
		invocations[0].Arguments[0].Should().Be("dark", "toggling from light must pass dark");
	}

	[Fact]
	public async Task ThemeBrightnessToggle_Toggle_WhenCurrentIsDark_CallsSelectBrightnessWithLight()
	{
		// click once (light → dark), then again (dark → light)
		SetupJsInteropForClick();
		var cut = Render<ThemeBrightnessToggleComponent>();
		await cut.Find("#nav-btn-brightness-toggle").ClickAsync(new());
		await cut.Find("#nav-btn-brightness-toggle").ClickAsync(new());
		var invocations = JSInterop.Invocations["ThemeManager.selectBrightnessAndUpdateUI"];
		invocations.Should().HaveCount(2);
		invocations[1].Arguments[0].Should().Be("light", "toggling from dark must pass light");
	}

	// ──────────────────────────────────────────────────────────────────────────
	// Isolation
	// ──────────────────────────────────────────────────────────────────────────

	[Fact]
	public void ThemeBrightnessToggle_RendersSuccessfully_WhenNoOtherThemeComponentPresent()
	{
		JSInterop.SetupVoid("ThemeManager.syncUI");
		var act = () => Render<ThemeBrightnessToggleComponent>();
		act.Should().NotThrow();
	}
}