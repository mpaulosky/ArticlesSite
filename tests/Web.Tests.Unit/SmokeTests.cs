// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     SmokeTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Unit
// =======================================================

namespace Web.Tests.Unit;

[ExcludeFromCodeCoverage]
public class SmokeTests
{

	[Fact]
	public void SmokeTest_ShouldPass()
	{
		true.Should().BeTrue();
	}

}