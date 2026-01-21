// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     RedirectToLoginTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Unit
// =======================================================

using Microsoft.AspNetCore.Components;

using Web.Components.Shared;

namespace Web.Tests.Unit.Components.Shared;

/// <summary>
/// Unit tests for RedirectToLogin using Bunit
/// </summary>
[ExcludeFromCodeCoverage]
public class RedirectToLoginTests : BunitContext
{
	[Fact]
	public void OnInitialized_NavigatesToLoginWithReturnUrl()
	{
		// Arrange
		var nav = new TestNavigationManager();
		nav.Uri = "https://localhost/somepage";
		Services.AddSingleton<NavigationManager>(nav);

		// Act
		var cut = Render<RedirectToLogin>();

		// Assert
		Assert.StartsWith("/Account/Login?returnUrl=", nav.LastNavigatedUri);
		Assert.True(nav.LastForceLoad);
	}

	private class TestNavigationManager : NavigationManager
	{
		public string LastNavigatedUri { get; private set; } = string.Empty;
		public bool LastForceLoad { get; private set; }
		public void SetUri(string uri) => Initialize(uri, uri);
		public new string Uri
		{
			get => base.Uri;
			set => SetUri(value);
		}
		protected override void NavigateToCore(string uri, bool forceLoad)
		{
			LastNavigatedUri = uri;
			LastForceLoad = forceLoad;
		}
	}
}
