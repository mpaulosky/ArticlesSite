// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     DependencyTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Architecture.Tests
// =======================================================

namespace Architecture.Tests;

/// <summary>
///   Tests to validate layer dependencies and architectural boundaries
/// </summary>
[ExcludeFromCodeCoverage]
public class DependencyTests
{

	private static readonly Assembly SharedAssembly = typeof(Article).Assembly;

	private static readonly Assembly WebAssembly = typeof(IAppMarker).Assembly;

	[Fact]
	public void Shared_ShouldNotReferenceWebProject()
	{
		// Arrange & Act
		IEnumerable<Type>? sharedTypes = Types.InAssembly(SharedAssembly).GetTypes();
		IEnumerable<Type>? webTypes = Types.InAssembly(WebAssembly).GetTypes();

		// Assert - Ensure no Shared types use Web types directly
		foreach (Type sharedType in sharedTypes)
		{
			IEnumerable<Type> fieldTypes = sharedType.GetFields().Select(f => f.FieldType);
			IEnumerable<Type> propertyTypes = sharedType.GetProperties().Select(p => p.PropertyType);
			IEnumerable<Type> allTypes = fieldTypes.Concat(propertyTypes);

			bool usesWebType = allTypes.Any(t => webTypes.Contains(t));

			usesWebType.Should().BeFalse($"{sharedType.Name} should not directly use Web types");
		}
	}

	[Fact]
	public void Shared_ShouldNotDependOnAspNetCore()
	{
		// Arrange & Act
		TestResult? result = Types.InAssembly(SharedAssembly)
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
		TestResult? result = Types.InAssembly(SharedAssembly)
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
		IEnumerable<Type>? interfaces = Types.InAssembly(SharedAssembly)
				.That()
				.ResideInNamespace("Shared.Interfaces")
				.GetTypes();

		// Assert
		foreach (Type type in interfaces)
		{
			type.IsInterface.Should().BeTrue($"{type.Name} in Interfaces namespace should be an interface");
		}
	}

	[Fact]
	public void WebComponents_ShouldNotDependOnDataLayer()
	{
		// Arrange & Act
		TestResult? result = Types.InAssembly(WebAssembly)
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
		IEnumerable<Type>? handlers = Types.InAssembly(WebAssembly)
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
		foreach (Type handler in handlers)
		{
			handler.Namespace.Should().Contain("Features", $"{handler.Name} should reside in Features namespace");
		}
	}

}
