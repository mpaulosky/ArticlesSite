//=======================================================
//Copyright (c) 2025. All rights reserved.
//File Name :     ConstantsTests.cs
//Company :       mpaulosky
//Author :        GitHub Copilot
//Solution Name : ArticlesSite
//Project Name :  Shared.Tests.Unit
//=======================================================

using System.Reflection;

using Shared.Constants;

namespace Shared.Tests.Unit.Constants;

[ExcludeFromCodeCoverage]
public class ConstantsTests
{
	[Fact]
	public void ConstantsClass_ShouldBePublicStatic()
	{
		var type = typeof(Shared.Constants.Constants);

		type.IsPublic.Should().BeTrue();
		type.IsAbstract.Should().BeTrue();
		type.IsSealed.Should().BeTrue(); // static class
	}

	[Fact]
	public void AllConstants_ShouldNotBeEmpty()
	{
		var fields = typeof(Shared.Constants.Constants).GetFields(BindingFlags.Public | BindingFlags.Static);

		fields.Should().NotBeEmpty();

		foreach (var f in fields)
		{
			f.FieldType.Should().Be(typeof(string));
			f.IsLiteral.Should().BeTrue();
			string? value = (string?)f.GetValue(null);
			value.Should().NotBeNullOrEmpty();
		}
	}

	[Theory]
	[InlineData("ArticleDatabase", "articlesdb")]
	[InlineData("Database", "articlesdb")]
	[InlineData("ServerName", "articlesite-server")]
	[InlineData("DefaultCorsPolicy", "DefaultPolicy")]
	[InlineData("ApiService", "api-service")]
	[InlineData("UserDatabase", "usersDb")]
	public void Constant_ShouldMatchExpectedValue(string constantName, string expectedValue)
	{
		var field = typeof(Shared.Constants.Constants).GetField(constantName, BindingFlags.Public | BindingFlags.Static);
		field.Should().NotBeNull();
		var value = (string?)field!.GetValue(null);
		value.Should().Be(expectedValue);
	}

}
