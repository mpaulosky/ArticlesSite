// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     NamingConventionTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Architecture.Tests.Unit
// =======================================================

namespace Architecture.Tests.Unit;

/// <summary>
///   Tests to validate naming conventions across the solution
/// </summary>
[ExcludeFromCodeCoverage]
public class NamingConventionTests
{

	private static readonly Assembly SharedAssembly = typeof(Shared.Entities.Article).Assembly;
	private static readonly Assembly WebAssembly = typeof(Web.IAppMarker).Assembly;

	[Fact]
	public void Entities_ShouldNotHaveDtoSuffix()
	{
		// Arrange & Act
		var result = Types.InAssembly(SharedAssembly)
			.That()
			.ResideInNamespace("Shared.Entities")
			.Should()
			.NotHaveNameEndingWith("Dto")
			.GetResult();

		// Assert
		result.IsSuccessful.Should().BeTrue("Entities should not have 'Dto' suffix");
	}

	[Fact]
	public void DTOs_ShouldHaveDtoSuffix()
	{
		// Arrange & Act
		var result = Types.InAssembly(SharedAssembly)
			.That()
			.ResideInNamespace("Shared.Models")
			.And()
			.AreClasses()
			.And()
			.DoNotHaveName("Result")
			.Should()
			.HaveNameEndingWith("Dto")
			.GetResult();

		// Assert
		result.IsSuccessful.Should().BeTrue("DTOs in Models namespace should have 'Dto' suffix");
	}

	[Fact]
	public void Interfaces_ShouldStartWithI()
	{
		// Arrange & Act
		var sharedResult = Types.InAssembly(SharedAssembly)
			.That()
			.AreInterfaces()
			.Should()
			.HaveNameStartingWith("I")
			.GetResult();

		var webResult = Types.InAssembly(WebAssembly)
			.That()
			.AreInterfaces()
			.Should()
			.HaveNameStartingWith("I")
			.GetResult();

		// Assert
		sharedResult.IsSuccessful.Should().BeTrue("Interfaces in Shared should start with 'I'");
		webResult.IsSuccessful.Should().BeTrue("Interfaces in Web should start with 'I'");
	}

	[Fact]
	public void Repositories_ShouldHaveRepositorySuffix()
	{
		// Arrange & Act
		var result = Types.InAssembly(SharedAssembly)
			.That()
			.ResideInNamespace("Shared.Interfaces")
			.And()
			.AreInterfaces()
			.And()
			.DoNotHaveName("IMongoDbContext")
			.And()
			.DoNotHaveName("IMongoDbContextFactory")
			.Should()
			.HaveNameEndingWith("Repository")
			.GetResult();

		// Assert
		result.IsSuccessful.Should().BeTrue("Repository interfaces should have 'Repository' suffix");
	}

	[Fact]
	public void Validators_ShouldHaveValidatorSuffix()
	{
		// Arrange & Act
		var result = Types.InAssembly(SharedAssembly)
			.That()
			.ResideInNamespace("Shared.Validators")
			.Should()
			.HaveNameEndingWith("Validator")
			.GetResult();

		// Assert
		result.IsSuccessful.Should().BeTrue("Validators should have 'Validator' suffix");
	}

	[Fact]
	public void RazorComponents_ShouldNotHaveComponentSuffix()
	{
		// Arrange & Act
		var components = Types.InAssembly(WebAssembly)
			.That()
			.ResideInNamespace("Web.Components")
			.And()
			.AreClasses()
			.GetTypes()
			.Where(t => t.Name.EndsWith("Component"));

		// Assert - Only specific shared components should have Component suffix
		var allowedComponentNames = new[] { "NavMenuComponent", "ConnectWithUsComponent", "ComponentHeadingComponent",
			"FooterComponent", "ErrorAlertComponent", "ErrorPageComponent", "LoginComponent", "PageHeaderComponent",
			"RecentRelatedComponent", "PostInfoComponent", "LoadingComponent", "PageHeadingComponent" }; foreach (var component in components)
		{
			allowedComponentNames.Should().Contain(component.Name,
				$"{component.Name} should be in the allowed list or renamed without 'Component' suffix");
		}
	}

}
