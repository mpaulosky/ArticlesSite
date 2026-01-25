//=======================================================
//Copyright (c) 2025. All rights reserved.
//File Name :     InvalidFolderExceptionTests.cs
//Company :       mpaulosky
//Author :        GitHub Copilot
//Solution Name : ArticlesSite
//Project Name :  Shared.Tests.Unit
//=======================================================

using System;
using System.Diagnostics.CodeAnalysis;

using Shared.FileStorage;

namespace Shared.Tests.Unit.FileStorage;

[ExcludeFromCodeCoverage]
public class InvalidFolderExceptionTests
{
	[Fact]
	public void DefaultConstructor_ShouldHaveDefaultMessage()
	{
		var ex = new InvalidFolderException();
		ex.Message.Should().Be("Invalid folder location.");
	}

	[Fact]
	public void MessageConstructor_ShouldPreserveMessage()
	{
		var ex = new InvalidFolderException("Custom message");
		ex.Message.Should().Be("Custom message");
	}

	[Fact]
	public void InnerExceptionConstructor_ShouldPreserveInnerException()
	{
		var inner = new Exception("inner");
		var ex = new InvalidFolderException("msg", inner);
		ex.InnerException.Should().Be(inner);
	}
}
