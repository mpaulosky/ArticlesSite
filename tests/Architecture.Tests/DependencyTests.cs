// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     DependencyTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Architecture.Tests.Unit
// =======================================================

namespace Architecture.Tests.Unit;

/// <summary>
///   Tests to validate layer dependencies and architectural boundaries
/// </summary>
[ExcludeFromCodeCoverage]
public class DependencyTests
{

	private static readonly Assembly SharedAssembly = typeof(Shared.Entities.Article).Assembly;
	private static readonly Assembly WebAssembly = typeof(Web.IAppMarker).Assembly;

	[Fact]
	public void Shared_ShouldNotReferenceWebProject()
	{
		// Arrange & Act
		var sharedTypes = Types.InAssembly(SharedAssembly).GetTypes();
		var webTypes = Types.InAssembly(WebAssembly).GetTypes();

		// Assert - Ensure no Shared types use Web types directly
		foreach (var sharedType in sharedTypes)
		{
			var fieldTypes = sharedType.GetFields().Select(f => f.FieldType);
			var propertyTypes = sharedType.GetProperties().Select(p => p.PropertyType);
			var allTypes = fieldTypes.Concat(propertyTypes);

			var usesWebType = allTypes.Any(t => webTypes.Contains(t));

			usesWebType.Should().BeFalse($"{sharedType.Name} should not directly use Web types");
		}
	}

	[Fact]
	public void Shared_ShouldNotDependOnAspNetCore()
	{
		// Arrange & Act
		var result = Types.InAssembly(SharedAssembly)
			.ShouldNot()
			.HaveDependencyOn("Microsoft.AspNetCore")
			.GetResult();

		// Assert
		result.IsSuccessful.Should().BeTrue("Shared layer should not depend on ASP.NET Core");
	}

	[Fact]
	public void Entities_ShouldNotDependOnModels()
	{
		// Arrange & Act
		var result = Types.InAssembly(SharedAssembly)
			.That()
			.ResideInNamespace("Shared.Entities")
			.ShouldNot()
			.HaveDependencyOn("Shared.Models")
			.GetResult();

		// Assert
		result.IsSuccessful.Should().BeTrue("Entities should not depend on Models (DTOs)");
	}

	[Fact]
	public void Interfaces_ShouldOnlyBeInInterfacesNamespace()
	{
		// Arrange & Act
		var interfaces = Types.InAssembly(SharedAssembly)
			.That()
			.ResideInNamespace("Shared.Interfaces")
			.GetTypes();

		// Assert
		foreach (var type in interfaces)
		{
			type.IsInterface.Should().BeTrue($"{type.Name} in Interfaces namespace should be an interface");
		}
	}

	[Fact]
	public void WebComponents_ShouldNotDependOnDataLayer()
	{
		// Arrange & Act
		var result = Types.InAssembly(WebAssembly)
			.That()
			.ResideInNamespace("Web.Components")
			.ShouldNot()
			.HaveDependencyOn("Web.Data")
			.GetResult();

		// Assert
		result.IsSuccessful.Should().BeTrue("Components should not directly depend on Data layer - use handlers instead");
	}

	[Fact]
	public void Handlers_ShouldResideInFeaturesNamespace()
	{
		// Arrange & Act
		var handlers = Types.InAssembly(WebAssembly)
			.That()
			.HaveNameEndingWith("Handler")
			.And()
			.DoNotHaveName("IGetArticlesHandler")
			.And()
			.DoNotHaveName("IGetArticleHandler")
			.And()
			.DoNotHaveName("ICreateArticleHandler")
			.And()
			.DoNotHaveName("IEditArticleHandler")
			.And()
			.DoNotHaveName("IGetCategoriesHandler")
			.And()
			.DoNotHaveName("IGetCategoryHandler")
			.And()
			.DoNotHaveName("ICreateCategoryHandler")
			.And()
			.DoNotHaveName("IEditCategoryHandler")
			.GetTypes();

		// Assert
		foreach (var handler in handlers)
		{
			handler.Namespace.Should().Contain("Features", $"{handler.Name} should reside in Features namespace");
		}
	}

}
