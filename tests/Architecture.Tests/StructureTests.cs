// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     StructureTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Architecture.Tests.Unit
// =======================================================

namespace Architecture.Tests.Unit;

/// <summary>
///   Tests to validate project structure and organization
/// </summary>
[ExcludeFromCodeCoverage]
public class StructureTests
{

	private static readonly Assembly SharedAssembly = typeof(Shared.Entities.Article).Assembly;
	private static readonly Assembly WebAssembly = typeof(Web.IAppMarker).Assembly;

	[Fact]
	public void Entities_ShouldBeSealed_OrAbstract()
	{
		// Arrange & Act
		var entities = Types.InAssembly(SharedAssembly)
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
		foreach (var entity in entities)
		{
			(entity.IsSealed || entity.IsAbstract).Should().BeTrue(
				$"{entity.Name} should be sealed or abstract to prevent unwanted inheritance");
		}
	}

	[Fact]
	public void DTOs_ShouldBeInModelsNamespace()
	{
		// Arrange & Act
		var dtos = Types.InAssembly(SharedAssembly)
			.That()
			.HaveNameEndingWith("Dto")
			.And()
			.DoNotResideInNamespace("Shared.Fakes")
			.GetTypes();

		// Assert
		foreach (var dto in dtos)
		{
			dto.Namespace.Should().Contain("Models", $"{dto.Name} should be in Models namespace");
		}
	}

	[Fact]
	public void Validators_ShouldInheritFromAbstractValidator()
	{
		// Arrange & Act
		var validators = Types.InAssembly(SharedAssembly)
			.That()
			.ResideInNamespace("Shared.Validators")
			.And()
			.AreClasses()
			.GetTypes();

		// Assert
		foreach (var validator in validators)
		{
			validator.BaseType?.Name.Should().StartWith("AbstractValidator",
				$"{validator.Name} should inherit from AbstractValidator<T>");
		}
	}

	[Fact]
	public void Repositories_ShouldBeInDataNamespace()
	{
		// Arrange & Act
		var repositories = Types.InAssembly(WebAssembly)
			.That()
			.HaveNameEndingWith("Repository")
			.And()
			.AreClasses()
			.GetTypes();

		// Assert
		foreach (var repository in repositories)
		{
			repository.Namespace.Should().Contain("Data",
				$"{repository.Name} should be in Data namespace");
		}
	}

	[Fact]
	public void Handlers_ShouldBeNestedInStaticClasses()
	{
		// Arrange & Act
		var handlers = Types.InAssembly(WebAssembly)
			.That()
			.HaveNameMatching("Handler$")
			.And()
			.AreClasses()
			.And()
			.ResideInNamespace("Web.Components.Features")
			.GetTypes();

		// Assert
		foreach (var handler in handlers)
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
		var handlers = Types.InAssembly(WebAssembly)
			.That()
			.HaveNameMatching("Handler$")
			.And()
			.AreClasses()
			.And()
			.ResideInNamespace("Web.Components.Features")
			.GetTypes();

		// Assert
		foreach (var handler in handlers)
		{
			handler.GetInterfaces().Should().NotBeEmpty(
				$"{handler.Name} should implement a handler interface");
		}
	}

	[Fact]
	public void Entities_ShouldHaveParameterlessConstructor()
	{
		// Arrange & Act
		var entities = Types.InAssembly(SharedAssembly)
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
		foreach (var entity in entities)
		{
			var hasParameterlessConstructor = entity.GetConstructors()
				.Any(c => c.GetParameters().Length == 0);

			hasParameterlessConstructor.Should().BeTrue(
				$"{entity.Name} should have a parameterless constructor for serialization");
		}
	}

}
