// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     StructureTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Architecture.Tests
// =======================================================

namespace Architecture.Tests;

/// <summary>
///   Tests to validate project structure and organization
/// </summary>
[ExcludeFromCodeCoverage]
public class StructureTests
{

	private static readonly Assembly SharedAssembly = typeof(Article).Assembly;

	private static readonly Assembly WebAssembly = typeof(IAppMarker).Assembly;

	[Fact]
	public void Entities_ShouldBeSealed_OrAbstract()
	{
		// Arrange & Act
		IEnumerable<Type>? entities = Types.InAssembly(SharedAssembly)
				.That()
				.ResideInNamespace("Shared.Entities")
				.And()
				.AreClasses()
				.And()
				.DoNotHaveName("Article") // Article is not sealed for flexibility
				.And()
				.DoNotHaveName("Category")
				.And()
				.DoNotHaveName("Author")
				.And()
				.DoNotHaveName("AuthorInfo") // Record type
				.And()
				.DoNotHaveName("Roles") // Static class
				.GetTypes();

		// Assert - Records and specific entities should be sealed or abstract
		foreach (Type entity in entities)
		{
			(entity.IsSealed || entity.IsAbstract).Should().BeTrue(
					$"{entity.Name} should be sealed or abstract to prevent unwanted inheritance");
		}
	}

	[Fact]
	public void DTOs_ShouldBeInModelsNamespace()
	{
		// Arrange & Act
		IEnumerable<Type>? dtos = Types.InAssembly(SharedAssembly)
				.That()
				.HaveNameEndingWith("Dto")
				.And()
				.DoNotResideInNamespace("Shared.Fakes")
				.GetTypes();

		// Assert
		foreach (Type dto in dtos)
		{
			dto.Namespace.Should().Contain("Models", $"{dto.Name} should be in Models namespace");
		}
	}

	[Fact]
	public void Validators_ShouldInheritFromAbstractValidator()
	{
		// Arrange & Act
		IEnumerable<Type>? validators = Types.InAssembly(SharedAssembly)
				.That()
				.ResideInNamespace("Shared.Validators")
				.And()
				.AreClasses()
				.GetTypes();

		// Assert
		foreach (Type validator in validators)
		{
			validator.BaseType?.Name.Should().StartWith("AbstractValidator",
					$"{validator.Name} should inherit from AbstractValidator<T>");
		}
	}

	[Fact]
	public void Repositories_ShouldBeInDataNamespace()
	{
		// Arrange & Act
		IEnumerable<Type>? repositories = Types.InAssembly(WebAssembly)
				.That()
				.HaveNameEndingWith("Repository")
				.And()
				.AreClasses()
				.GetTypes();

		// Assert
		foreach (Type repository in repositories)
		{
			repository.Namespace.Should().Contain("Data",
					$"{repository.Name} should be in Data namespace");
		}
	}

	[Fact]
	public void Handlers_ShouldBeNestedInStaticClasses()
	{
		// Arrange & Act
		IEnumerable<Type>? handlers = Types.InAssembly(WebAssembly)
				.That()
				.HaveNameMatching("Handler$")
				.And()
				.AreClasses()
				.And()
				.ResideInNamespace("Web.Components.Features")
				.GetTypes();

		// Assert
		foreach (Type handler in handlers)
		{
			handler.DeclaringType.Should().NotBeNull($"{handler.Name} should be nested in a static class");
			handler.DeclaringType!.IsAbstract.Should().BeTrue($"{handler.Name} should be nested in a static class");
			handler.DeclaringType.IsSealed.Should().BeTrue($"{handler.Name} should be nested in a static class");
		}
	}

	[Fact]
	public void FeatureHandlers_ShouldImplementInterface()
	{
		// Arrange & Act
		IEnumerable<Type>? handlers = Types.InAssembly(WebAssembly)
				.That()
				.HaveNameMatching("Handler$")
				.And()
				.AreClasses()
				.And()
				.ResideInNamespace("Web.Components.Features")
				.GetTypes();

		// Assert
		foreach (Type handler in handlers)
		{
			handler.GetInterfaces().Should().NotBeEmpty(
					$"{handler.Name} should implement a handler interface");
		}
	}

	[Fact]
	public void Entities_ShouldHaveParameterlessConstructor()
	{
		// Arrange & Act
		IEnumerable<Type>? entities = Types.InAssembly(SharedAssembly)
				.That()
				.ResideInNamespace("Shared.Entities")
				.And()
				.AreClasses()
				.And()
				.DoNotHaveName("AuthorInfo") // Record type
				.And()
				.DoNotHaveName("Roles") // Static class
				.GetTypes();

		// Assert
		foreach (Type entity in entities)
		{
			bool hasParameterlessConstructor = entity.GetConstructors()
					.Any(c => c.GetParameters().Length == 0);

			hasParameterlessConstructor.Should().BeTrue(
					$"{entity.Name} should have a parameterless constructor for serialization");
		}
	}

}
