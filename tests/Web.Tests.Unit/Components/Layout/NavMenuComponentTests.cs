// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     NavMenuComponentTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Unit
// =======================================================

using Microsoft.AspNetCore.Authorization;

namespace Web.Components.Layout;

/// <summary>
/// Unit tests for NavMenuComponent using bUnit.
/// Covers integration of ThemeBrightnessToggleComponent and ThemeColorDropdownComponent
/// inside the nav bar, including mobile-hiding behaviour.
///
/// Test list items covered:
///   Item 5 — theme controls present so colour changes propagate via shared ThemeManager
///   Item 6 — brightness toggle present so brightness changes propagate via shared ThemeManager
///   Item 8 — nav theme controls hidden on mobile (hidden sm:flex wrapper)
/// </summary>
[ExcludeFromCodeCoverage]
public class NavMenuComponentTests : BunitContext
{
	public NavMenuComponentTests()
	{
		// Auth services required by AuthorizeView used inside the nav
		Services.AddAuthorization();
		Services.AddSingleton<IAuthorizationService, AlwaysAllowAuthorizationService>();
		Services.AddSingleton<AuthenticationStateProvider>(
			new TestAuthStateProvider(isAuthenticated: false));

		// JS interop required by the two child theme components
		JSInterop.Setup<string>("ThemeManager.getCurrentColor").SetResult("BLUE");
		JSInterop.Setup<string>("ThemeManager.getCurrentBrightness").SetResult("light");
		JSInterop.SetupVoid("ThemeManager.syncUI");
	}

	// ──────────────────────────────────────────────────────────────────────────
	// Test list item 8 — theme controls hidden on mobile, visible on sm+
	// ──────────────────────────────────────────────────────────────────────────

	/// <summary>
	/// The wrapper div that holds both theme components must carry
	/// "hidden sm:flex" so it collapses on small screens exactly like the nav links.
	/// </summary>
	[Fact]
	public void NavMenu_ThemeControls_Wrapper_HasHiddenSmFlexClasses()
	{
		// Act
		var cut = Render<CascadingAuthenticationState>(
			parameters => parameters.AddChildContent<NavMenuComponent>());

		// Assert
		var themeWrapper = cut.Find(".hidden.sm\\:flex");
		themeWrapper.Should().NotBeNull("the theme controls must be wrapped in 'hidden sm:flex' to hide on mobile");
	}

	[Fact]
	public void NavMenu_ThemeControls_Wrapper_HasItemsCenter_And_Gap2_Classes()
	{
		// Act
		var cut = Render<CascadingAuthenticationState>(
			parameters => parameters.AddChildContent<NavMenuComponent>());

		// Assert
		var themeWrapper = cut.Find(".hidden.sm\\:flex");
		themeWrapper.ClassList.Should().Contain("items-center");
		themeWrapper.ClassList.Should().Contain("gap-2");
	}

	// ──────────────────────────────────────────────────────────────────────────
	// Test list items 5 & 6 — both theme components are rendered in the nav
	// (their presence ensures changes propagate through the shared ThemeManager)
	// ──────────────────────────────────────────────────────────────────────────

	/// <summary>
	/// Test list item 6 — brightness toggle is rendered in the nav.
	/// </summary>
	[Fact]
	public void NavMenu_Renders_ThemeBrightnessToggleComponent()
	{
		// Act
		var cut = Render<CascadingAuthenticationState>(
			parameters => parameters.AddChildContent<NavMenuComponent>());

		// Assert — toggle button is present
		cut.Find("#nav-btn-brightness-toggle").Should().NotBeNull(
			"ThemeBrightnessToggleComponent must be rendered so brightness changes sync with ThemeSelector");
	}

	/// <summary>
	/// Test list item 5 — colour dropdown is rendered in the nav.
	/// </summary>
	[Fact]
	public void NavMenu_Renders_ThemeColorDropdownComponent()
	{
		// Act
		var cut = Render<CascadingAuthenticationState>(
			parameters => parameters.AddChildContent<NavMenuComponent>());

		// Assert — colour dropdown is present
		cut.Find(".theme-color-dropdown").Should().NotBeNull(
			"ThemeColorDropdownComponent must be rendered so colour changes sync with ThemeSelector");
	}

	[Fact]
	public void NavMenu_Renders_BothThemeComponents_InsideTheSameWrapper()
	{
		// Act
		var cut = Render<CascadingAuthenticationState>(
			parameters => parameters.AddChildContent<NavMenuComponent>());

		// Assert — both components sit within the same hidden sm:flex wrapper
		var themeWrapper = cut.Find(".hidden.sm\\:flex");
		themeWrapper.QuerySelector("#nav-btn-brightness-toggle").Should().NotBeNull(
			"brightness toggle must be inside the hidden sm:flex div");
		themeWrapper.QuerySelector(".theme-color-dropdown").Should().NotBeNull(
			"colour dropdown must be inside the hidden sm:flex div");
	}

	/// <summary>
	/// Brightness toggle renders one button inside the nav.
	/// </summary>
	[Fact]
	public void NavMenu_ThemeBrightnessToggle_RendersOneButton()
	{
		// Act
		var cut = Render<CascadingAuthenticationState>(
			parameters => parameters.AddChildContent<NavMenuComponent>());

		// Assert
		var brightnessButtons = cut.FindAll(".brightness-btn");
		brightnessButtons.Should().HaveCount(1);
	}

	/// <summary>
	/// Colour dropdown renders 4 options inside the nav.
	/// </summary>
	[Fact]
	public void NavMenu_ThemeColorDropdown_RendersAllFourOptions()
	{
		// Act
		var cut = Render<CascadingAuthenticationState>(
			parameters => parameters.AddChildContent<NavMenuComponent>());

		// Assert
		cut.Find(".theme-color-dropdown").QuerySelectorAll("option").Should().HaveCount(4);
	}

	/// <summary>
	/// Test list item 5 — clicking the colour dropdown calls the same JS function
	/// that ThemeSelector uses, ensuring cross-component state sync.
	/// </summary>
	[Fact]
	public async Task NavMenu_ColorDropdown_OnChange_CallsSelectColorAndUpdateUI()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.selectColorAndUpdateUI", _ => true);

		var cut = Render<CascadingAuthenticationState>(
			parameters => parameters.AddChildContent<NavMenuComponent>());

		// Act
		await cut.Find(".theme-color-dropdown").ChangeAsync(new() { Value = "RED" });

		// Assert — same function ThemeSelector uses; syncUI will update .color-btn active states
		JSInterop.VerifyInvoke("ThemeManager.selectColorAndUpdateUI");
		JSInterop.Invocations["ThemeManager.selectColorAndUpdateUI"][0]
			.Arguments[0].Should().Be("RED");
	}

	/// <summary>
	/// Test list item 6 — clicking the brightness toggle calls the same JS function
	/// that ThemeSelector uses, ensuring cross-component state sync.
	/// </summary>
	[Fact]
	public async Task NavMenu_BrightnessButtons_OnClick_CallsSelectBrightnessAndUpdateUI()
	{
		// Arrange
		JSInterop.SetupVoid("ThemeManager.selectBrightnessAndUpdateUI", _ => true);

		var cut = Render<CascadingAuthenticationState>(
			parameters => parameters.AddChildContent<NavMenuComponent>());
		await cut.InvokeAsync(() => Task.CompletedTask); // let OnAfterRenderAsync complete first

		// Act — default brightness is "light", so toggle should call with "dark"
		await cut.Find("#nav-btn-brightness-toggle").ClickAsync(new());

		// Assert — same function ThemeSelector uses; syncUI will update .brightness-btn active states
		JSInterop.VerifyInvoke("ThemeManager.selectBrightnessAndUpdateUI");
		JSInterop.Invocations["ThemeManager.selectBrightnessAndUpdateUI"][0]
			.Arguments[0].Should().Be("dark");
	}

	// ──────────────────────────────────────────────────────────────────────────
	// Helpers
	// ──────────────────────────────────────────────────────────────────────────

	private sealed class AlwaysAllowAuthorizationService : IAuthorizationService
	{
		public Task<AuthorizationResult> AuthorizeAsync(
			ClaimsPrincipal user, object? resource, IEnumerable<IAuthorizationRequirement> requirements)
			=> Task.FromResult(
				user.Identity?.IsAuthenticated == true
					? AuthorizationResult.Success()
					: AuthorizationResult.Failed());

		public Task<AuthorizationResult> AuthorizeAsync(
			ClaimsPrincipal user, object? resource, string policyName)
			=> Task.FromResult(
				user.Identity?.IsAuthenticated == true
					? AuthorizationResult.Success()
					: AuthorizationResult.Failed());
	}

	private sealed class TestAuthStateProvider : AuthenticationStateProvider
	{
		private readonly bool _isAuthenticated;

		public TestAuthStateProvider(bool isAuthenticated) =>
			_isAuthenticated = isAuthenticated;

		public override Task<AuthenticationState> GetAuthenticationStateAsync()
		{
			var identity = _isAuthenticated
				? new ClaimsIdentity([new Claim("name", "TestUser")], "TestAuth")
				: new ClaimsIdentity();

			return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
		}
	}
}
