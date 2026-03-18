// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     MainLayoutTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Unit
// =======================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace Web.Components.Layout;

/// <summary>
/// Unit tests for MainLayout component
/// </summary>
[ExcludeFromCodeCoverage]
public class MainLayoutTests
{
	[Fact]
	public void GetErrorCode_WithNullException_ShouldReturn500()
	{
		// Arrange
		Exception? exception = null;

		// Act
		var result = InvokeGetErrorCode(exception);

		// Assert
		result.Should().Be(500);
	}

	[Fact]
	public void GetErrorCode_WithUnauthorizedAccessException_ShouldReturn401()
	{
		// Arrange
		var exception = new UnauthorizedAccessException();

		// Act
		var result = InvokeGetErrorCode(exception);

		// Assert
		result.Should().Be(401);
	}

	[Fact]
	public void GetErrorCode_WithArgumentException_ShouldReturn400()
	{
		// Arrange
		var exception = new ArgumentException();

		// Act
		var result = InvokeGetErrorCode(exception);

		// Assert
		result.Should().Be(400);
	}

	[Fact]
	public void GetErrorCode_WithKeyNotFoundException_ShouldReturn404()
	{
		// Arrange
		var exception = new KeyNotFoundException();

		// Act
		var result = InvokeGetErrorCode(exception);

		// Assert
		result.Should().Be(404);
	}

	[Fact]
	public void GetErrorCode_WithInvalidOperationException_ShouldReturn500()
	{
		// Arrange
		var exception = new InvalidOperationException();

		// Act
		var result = InvokeGetErrorCode(exception);

		// Assert
		result.Should().Be(500);
	}

	[Fact]
	public void GetErrorCode_WithGenericException_ShouldReturn500()
	{
		// Arrange
		var exception = new Exception("Generic error");

		// Act
		var result = InvokeGetErrorCode(exception);

		// Assert
		result.Should().Be(500);
	}

	// Helper method to test the private static GetErrorCode method via reflection
	private static int InvokeGetErrorCode(Exception? exception)
	{
		var mainLayoutType = typeof(MainLayout);
		var method = mainLayoutType.GetMethod("GetErrorCode",
			BindingFlags.NonPublic | BindingFlags.Static);

		method.Should().NotBeNull("GetErrorCode method should exist");

		var result = method.Invoke(null, [ exception ]);
		return (int)result!;
	}
}

[ExcludeFromCodeCoverage]
public class MainLayoutComponentTests : BunitContext
{
	public MainLayoutComponentTests()
	{
		// Register minimal authorization services required by AuthorizeView used in layout
		Services.AddSingleton<IAuthorizationPolicyProvider>(new DefaultAuthorizationPolicyProvider(Options.Create(new AuthorizationOptions())));
		var authService = Substitute.For<IAuthorizationService>();
		// Ensure authorization checks succeed in unit tests
		authService.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), Arg.Any<string>())
			.Returns(Task.FromResult(AuthorizationResult.Success()));
		authService.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), Arg.Any<IEnumerable<IAuthorizationRequirement>>())
			.Returns(Task.FromResult(AuthorizationResult.Success()));
		Services.AddSingleton(authService);
		// Register a fake authentication state provider and cascading auth state
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", [ "Admin" ]);
	}
	[Fact]
	public async Task OnAfterRender_FirstRender_SetsInitializedFlag()
	{
		// Act - invoke lifecycle directly without rendering to avoid nested component auth dependencies
		var instance = new MainLayout();
		var method = instance.GetType().GetMethod("OnAfterRenderAsync", BindingFlags.NonPublic | BindingFlags.Instance);
		try
		{
			if (method?.Invoke(instance, [ true ]) is Task task) await task;
		}
		catch (TargetInvocationException tie) when (tie.InnerException is InvalidOperationException ioe && ioe.Message.Contains("render handle is not yet assigned"))
		{
			// Expected when invoking lifecycle outside of a render host; continue
		}

		// Assert - private field _initialized should be true after invocation
		var field = instance.GetType().GetField("_initialized", BindingFlags.NonPublic | BindingFlags.Instance);
		var value = field?.GetValue(instance);
		((bool)value!).Should().BeTrue();
	}

	[Fact]
	public async Task OnAfterRender_SecondCall_DoesNotResetInitialized()
	{
		var instance = new MainLayout();
		var method = instance.GetType().GetMethod("OnAfterRenderAsync", BindingFlags.NonPublic | BindingFlags.Instance);
		try
		{
			// First render
			if (method?.Invoke(instance, [ true ]) is Task first) await first;

			// Second render (should not reset)
			if (method?.Invoke(instance, [ false ]) is Task second) await second;
		}
		catch (TargetInvocationException tie) when (tie.InnerException is InvalidOperationException ioe && ioe.Message.Contains("render handle is not yet assigned"))
		{
			// Expected when invoking lifecycle outside of a render host; continue
		}

		var field = instance.GetType().GetField("_initialized", BindingFlags.NonPublic | BindingFlags.Instance);
		var value = field?.GetValue(instance);
		((bool)value!).Should().BeTrue();
	}

	// Skipping rendering-based UI assertions here because MainLayout contains authorization-wrapped components
	// which require a full auth rendering environment. Those UI elements are covered in their respective
	// component tests (e.g., FooterComponent, ErrorBoundary integration tests).
	[Fact]
	public void Dispose_CanBeCalledWithoutException()
	{
		var cut = RenderWithAuth();
		cut.Instance.Invoking(i => i.Dispose()).Should().NotThrow();
	}

	private IRenderedComponent<MainLayout> RenderWithAuth()
	{
		var frag = Render(builder =>
		{
			// Provide authentication state as a cascading value directly to avoid provider timing issues
			builder.OpenComponent<CascadingValue<Task<AuthenticationState>>>(0);
			builder.AddAttribute(1, "Value", Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(
					[ new Claim(ClaimTypes.Name, "test") ], "FakeAuth")))));
			builder.AddAttribute(2, "ChildContent", (RenderFragment)(b =>
			{
				b.OpenComponent<MainLayout>(3);
				b.CloseComponent();
			}));
			builder.CloseComponent();
		});

		return frag.FindComponent<MainLayout>();
	}
}
