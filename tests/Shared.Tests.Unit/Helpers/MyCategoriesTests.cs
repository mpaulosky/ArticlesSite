//=======================================================
//Copyright (c) 2025. All rights reserved.
//File Name :     MyCategoriesTests.cs
//Company :       mpaulosky
//Author :        GitHub Copilot
//Solution Name : ArticlesSite
//Project Name :  Shared.Tests.Unit
//=======================================================

using System.Reflection;

using Shared.Helpers;

namespace Shared.Tests.Unit.Helpers;

[ExcludeFromCodeCoverage]
public class MyCategoriesTests
{
	[Fact]
	public void MyCategories_ShouldContainExpectedConstants()
	{
		var type = typeof(MyCategories);

		var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
		fields.Length.Should().Be(9);

		MyCategories.First.Should().Be("ASP.NET Core");
		MyCategories.Second.Should().Be("Blazor Server");
		MyCategories.Third.Should().Be("Blazor WebAssembly");
		MyCategories.Fourth.Should().Be("C# Programming");
		MyCategories.Fifth.Should().Be("Entity Framework Core (EF Core)");
		MyCategories.Sixth.Should().Be(".NET MAUI");
		MyCategories.Seventh.Should().Be("General Programming");
		MyCategories.Eighth.Should().Be("Web Development");
		MyCategories.Ninth.Should().Be("Other .NET Topics");
	}
}
