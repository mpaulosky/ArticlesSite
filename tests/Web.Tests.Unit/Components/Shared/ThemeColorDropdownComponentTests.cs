// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ThemeColorDropdownComponentTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Unit
// =======================================================

namespace Web.Components.Shared;

/// <summary>
/// Unit tests for ThemeColorDropdownComponent using bUnit.
/// Covers rendering (4 emoji options), on-load pre-selection, and colour-change JS interop.
///
/// JS-side effects (HTML class and localStorage update) are verified by asserting the exact
/// arguments passed to ThemeManager.selectColorAndUpdateUI / ThemeManager.getCurrentColor,
/// because bUnit does not execute real JavaScript.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeColorDropdownComponentTests : BunitContext
{
	// ──────────────────────────────────────────────────────────────────────────
	// Rendering
	// ──────────────────────────────────────────────────────────────────────────

	/// <summary>
	/// Test list item 9 — component renders a &lt;select&gt; with exactly 4 colour options.
	/// </summary>
	[Fact]
	public void ThemeColorDropdown_Renders_SelectElement()
	{
		// Arrange
		JSInterop.Setup<string>("ThemeManager.getCurrentColor").SetResult("BLUE");

		// Act
		var cut = Render<ThemeColorDropdownComponent>();

		// Assert
		cut.Find("select").Should().NotBeNull();
	}

	[Fact]
	public void ThemeColorDropdown_Renders_ExactlyFourOptions()
	{
		// Arrange
		JSInterop.Setup<string>("ThemeManager.getCurrentColor").SetResult("BLUE");

		// Act
		var cut = Render<ThemeColorDropdownComponent>();

		// Assert
		cut.FindAll("option").Should().HaveCount(4);
	}

	[Fact]
	public void ThemeColorDropdown_Options_HaveCorrectValues()
	{
		// Arrange
		JSInterop.Setup<string>("ThemeManager.getCurrentColor").SetResult("BLUE");

		// Act
		var cut = Render<ThemeColorDropdownComponent>();

		// Assert
		var options = cut.FindAll("option");
		options.Select(o => o.GetAttribute("value")).Should()
			.BeEquivalentTo(["RED", "BLUE", "GREEN", "YELLOW"]);
	}

	/// <summary>
	/// Test list decision 2 — options carry emoji colour indicators.
	/// </summary>
	[Theory]
	[InlineData("RED", "🔴")]
	[InlineData("BLUE", "🔵")]
	[InlineData("GREEN", "🟢")]
	[InlineData("YELLOW", "🟡")]
	public void ThemeColorDropdown_Options_ContainEmojiAndLabel(string value, string expectedEmoji)
	{
		// Arrange
		JSInterop.Setup<string>("ThemeManager.getCurrentColor").SetResult("BLUE");

		// Act
		var cut = Render<ThemeColorDropdownComponent>();

		// Assert
		var option = cut.Find($"option[value='{value}']");
		option.TextContent.Should().Contain(expectedEmoji, $"option {value} must show the {expectedEmoji} emoji");
	}

	[Fact]
	public void ThemeColorDropdown_Select_HasThemeColorDropdownClass()
	{
		// Arrange
		JSInterop.Setup<string>("ThemeManager.getCurrentColor").SetResult("BLUE");

		// Act
		var cut = Render<ThemeColorDropdownComponent>();

		// Assert
		cut.Find("select").ClassList.Should().Contain("theme-color-dropdown");
	}

	// ──────────────────────────────────────────────────────────────────────────
	// Test list item 4 — dropdown pre-selects the persisted colour on load
	// ──────────────────────────────────────────────────────────────────────────

	[Fact]
	public async Task ThemeColorDropdown_OnAfterRenderAsync_CallsGetCurrentColor()
	{
		// Arrange
		JSInterop.Setup<string>("ThemeManager.getCurrentColor").SetResult("BLUE");

		// Act
		var cut = Render<ThemeColorDropdownComponent>();
		await cut.InvokeAsync(() => Task.CompletedTask);

		// Assert
		JSInterop.Invocations["ThemeManager.getCurrentColor"].Should()
			.HaveCount(1, "getCurrentColor must be called once on first render to pre-select the persisted colour");
	}

	[Theory]
	[InlineData("RED")]
	[InlineData("BLUE")]
	[InlineData("GREEN")]
	[InlineData("YELLOW")]
	public async Task ThemeColorDropdown_OnAfterRenderAsync_PreSelectsActiveColour(string persistedColor)
	{
		// Arrange
		JSInterop.Setup<string>("ThemeManager.getCurrentColor").SetResult(persistedColor);

		// Act
		var cut = Render<ThemeColorDropdownComponent>();
		await cut.InvokeAsync(() => Task.CompletedTask);

		// Assert — the select value attribute must reflect the persisted colour
		cut.Find("select").GetAttribute("value").Should().Be(persistedColor,
			$"the dropdown must pre-select \"{persistedColor}\" to reflect the persisted theme");
	}

	[Fact]
	public void ThemeColorDropdown_DefaultSelectedColor_IsBLUE_BeforeFirstRender()
	{
		// Arrange
		JSInterop.Setup<string>("ThemeManager.getCurrentColor").SetResult("BLUE");

		// Act — component renders synchronously; JS result not applied until after render
		var cut = Render<ThemeColorDropdownComponent>();

		// Assert — BLUE is the hard-coded field default
		cut.Find("select").GetAttribute("value").Should().Be("BLUE");
	}

	// ──────────────────────────────────────────────────────────────────────────
	// Test list items 3 & 5 — changing colour calls selectColorAndUpdateUI
	// (ThemeManager.selectColorAndUpdateUI updates localStorage["tailwind-color-theme"],
	// applies the CSS class on <html>, and calls syncUI which updates ALL .color-btn
	// elements in the DOM — including those in the full ThemeSelector card)
	// ──────────────────────────────────────────────────────────────────────────

	[Theory]
	[InlineData("RED")]
	[InlineData("BLUE")]
	[InlineData("GREEN")]
	[InlineData("YELLOW")]
	public async Task ThemeColorDropdown_OnChange_CallsSelectColorAndUpdateUI_WithCorrectArg(string selectedColor)
	{
		// Arrange
		JSInterop.Setup<string>("ThemeManager.getCurrentColor").SetResult("BLUE");
		JSInterop.SetupVoid("ThemeManager.selectColorAndUpdateUI", _ => true);

		var cut = Render<ThemeColorDropdownComponent>();

		// Act
		await cut.Find("select").ChangeAsync(new() { Value = selectedColor });

		// Assert
		JSInterop.VerifyInvoke("ThemeManager.selectColorAndUpdateUI");
		var invocations = JSInterop.Invocations["ThemeManager.selectColorAndUpdateUI"];
		invocations.Should().HaveCount(1);
		invocations[0].Arguments[0].Should().Be(selectedColor,
			$"selecting \"{selectedColor}\" must pass exactly \"{selectedColor}\" to ThemeManager, " +
			"preserving the current brightness and updating localStorage + html class + all .color-btn active states");
	}

	/// <summary>
	/// Verifies that ThemeManager.getCurrentColor is called only once — on first render —
	/// not again when the user changes the dropdown selection.
	/// </summary>
	[Fact]
	public async Task ThemeColorDropdown_GetCurrentColor_CalledOnlyOnFirstRender_NotOnSubsequentChanges()
	{
		// Arrange
		JSInterop.Setup<string>("ThemeManager.getCurrentColor").SetResult("BLUE");
		JSInterop.SetupVoid("ThemeManager.selectColorAndUpdateUI", _ => true);

		var cut = Render<ThemeColorDropdownComponent>();
		await cut.InvokeAsync(() => Task.CompletedTask); // allow first render to settle

		// Act — change the dropdown to trigger a re-render cycle
		await cut.Find("select").ChangeAsync(new() { Value = "RED" });

		// Assert — getCurrentColor was still called exactly once
		JSInterop.Invocations["ThemeManager.getCurrentColor"].Should()
			.HaveCount(1, "ThemeManager.getCurrentColor must not be called again after the initial load");
	}

	[Fact]
	public async Task ThemeColorDropdown_OnChange_DoesNotCallSelectBrightness()
	{
		// Arrange
		JSInterop.Setup<string>("ThemeManager.getCurrentColor").SetResult("BLUE");
		JSInterop.SetupVoid("ThemeManager.selectColorAndUpdateUI", _ => true);

		var cut = Render<ThemeColorDropdownComponent>();

		// Act
		await cut.Find("select").ChangeAsync(new() { Value = "RED" });

		// Assert — colour change must not alter brightness
		JSInterop.Invocations
			.Any(i => i.Identifier == "ThemeManager.selectBrightnessAndUpdateUI")
			.Should().BeFalse("changing colour must preserve brightness — selectBrightnessAndUpdateUI should not be called");
	}

	[Fact]
	public async Task ThemeColorDropdown_OnChange_WithNullValue_DefaultsToBLUE()
	{
		// Arrange
		JSInterop.Setup<string>("ThemeManager.getCurrentColor").SetResult("BLUE");
		JSInterop.SetupVoid("ThemeManager.selectColorAndUpdateUI", _ => true);

		var cut = Render<ThemeColorDropdownComponent>();

		// Act — simulate a null value coming through ChangeEventArgs
		await cut.Find("select").ChangeAsync(new() { Value = null });

		// Assert — null value falls back to BLUE via the null-coalescing operator
		var invocations = JSInterop.Invocations["ThemeManager.selectColorAndUpdateUI"];
		invocations.Should().HaveCount(1);
		invocations[0].Arguments[0].Should().Be("BLUE");
	}

	// ──────────────────────────────────────────────────────────────────────────
	// Test list item 7 — renders successfully without conflicting with other components
	// ──────────────────────────────────────────────────────────────────────────

	[Fact]
	public void ThemeColorDropdown_RendersSuccessfully_WhenNoOtherThemeComponentPresent()
	{
		// Arrange
		JSInterop.Setup<string>("ThemeManager.getCurrentColor").SetResult("BLUE");

		// Act
		var act = () => Render<ThemeColorDropdownComponent>();

		// Assert
		act.Should().NotThrow();
	}
}
