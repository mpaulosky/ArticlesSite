//=======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     FakeAuthorInfoTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Shared.Tests.Unit
// =======================================================

using Shared.Entities;

namespace Shared.Tests.Unit.Fakes;

[ExcludeFromCodeCoverage]
public class FakeAuthorInfoTests
{
	[Fact]
	public void GetNewAuthorInfo_WithoutSeed_ShouldReturnValidAuthorInfo()
	{
		// Arrange & Act
		AuthorInfo result = FakeAuthorInfo.GetNewAuthorInfo();

		// Assert
		result.Should().NotBeNull();
		result.UserId.Should().NotBeNullOrWhiteSpace();
		result.Name.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public void GetNewAuthorInfo_WithSeed_ShouldReturnDeterministicAuthorInfo()
	{
		// Arrange & Act
		AuthorInfo result1 = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);
		AuthorInfo result2 = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);

		// Assert
		result1.Should().NotBeNull();
		result2.Should().NotBeNull();
		result1.UserId.Should().Be(result2.UserId);
		result1.Name.Should().Be(result2.Name);
	}

	[Fact]
	public void GetNewAuthorInfo_WithoutSeed_ShouldReturnDifferentAuthorInfos()
	{
		// Arrange & Act
		AuthorInfo result1 = FakeAuthorInfo.GetNewAuthorInfo();
		AuthorInfo result2 = FakeAuthorInfo.GetNewAuthorInfo();

		// Assert
		result1.Should().NotBeNull();
		result2.Should().NotBeNull();
		result1.UserId.Should().NotBe(result2.UserId);
		result1.Name.Should().NotBe(result2.Name);
	}

	[Fact]
	public void GetAuthorInfos_WithZeroCount_ShouldReturnEmptyList()
	{
		// Arrange & Act
		List<AuthorInfo> result = FakeAuthorInfo.GetAuthorInfos(0);

		// Assert
		result.Should().NotBeNull();
		result.Should().BeEmpty();
	}

	[Fact]
	public void GetAuthorInfos_WithPositiveCount_ShouldReturnCorrectNumberOfAuthorInfos()
	{
		// Arrange
		const int count = 5;

		// Act
		List<AuthorInfo> result = FakeAuthorInfo.GetAuthorInfos(count);

		// Assert
		result.Should().NotBeNull();
		result.Should().HaveCount(count);
		result.Should().OnlyContain(a => !string.IsNullOrWhiteSpace(a.UserId));
		result.Should().OnlyContain(a => !string.IsNullOrWhiteSpace(a.Name));
	}

	[Fact]
	public void GetAuthorInfos_WithSeed_ShouldReturnDeterministicList()
	{
		// Arrange
		const int count = 3;

		// Act
		List<AuthorInfo> result1 = FakeAuthorInfo.GetAuthorInfos(count, useSeed: true);
		List<AuthorInfo> result2 = FakeAuthorInfo.GetAuthorInfos(count, useSeed: true);

		// Assert
		result1.Should().HaveCount(count);
		result2.Should().HaveCount(count);

		for (int i = 0; i < count; i++)
		{
			result1[i].UserId.Should().Be(result2[i].UserId);
			result1[i].Name.Should().Be(result2[i].Name);
		}
	}

	[Fact]
	public void GetAuthorInfos_WithoutSeed_ShouldReturnDifferentLists()
	{
		// Arrange
		const int count = 3;

		// Act
		List<AuthorInfo> result1 = FakeAuthorInfo.GetAuthorInfos(count);
		List<AuthorInfo> result2 = FakeAuthorInfo.GetAuthorInfos(count);

		// Assert
		result1.Should().HaveCount(count);
		result2.Should().HaveCount(count);
		result1[0].UserId.Should().NotBe(result2[0].UserId);
	}

	[Fact]
	public void GetAuthorInfos_ShouldReturnUniqueAuthorInfosInList()
	{
		// Arrange
		const int count = 10;

		// Act
		List<AuthorInfo> result = FakeAuthorInfo.GetAuthorInfos(count);

		// Assert
		result.Should().HaveCount(count);
		result.Select(a => a.UserId).Distinct().Should().HaveCount(count, "all author infos should have unique user IDs");
		result.Select(a => a.Name).Distinct().Should().HaveCount(count, "all author infos should have unique names");
	}

	[Fact]
	public void GetAuthorInfos_AllAuthorInfos_ShouldHaveValidProperties()
	{
		// Arrange
		const int count = 5;

		// Act
		List<AuthorInfo> result = FakeAuthorInfo.GetAuthorInfos(count);

		// Assert
		result.Should().OnlyContain(a => !string.IsNullOrWhiteSpace(a.UserId));
		result.Should().OnlyContain(a => !string.IsNullOrWhiteSpace(a.Name));
	}

	[Fact]
	public void GenerateFake_WithoutSeed_ShouldReturnConfiguredFaker()
	{
		// Arrange & Act
		AuthorInfo result = FakeAuthorInfo.GetNewAuthorInfo();

		// Assert
		result.Should().NotBeNull();
		result.UserId.Should().NotBeNullOrWhiteSpace();
		result.Name.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public void GenerateFake_WithSeed_ShouldProduceDeterministicResults()
	{
		// Arrange & Act
		AuthorInfo result1 = FakeAuthorInfo.GetNewAuthorInfo(true);
		AuthorInfo result2 = FakeAuthorInfo.GetNewAuthorInfo(true);

		// Assert
		result1.UserId.Should().Be(result2.UserId);
		result1.Name.Should().Be(result2.Name);
	}

	[Fact]
	public void GetNewAuthorInfo_UserId_ShouldBeValidGuidFormat()
	{
		// Arrange & Act
		AuthorInfo result = FakeAuthorInfo.GetNewAuthorInfo();

		// Assert
		result.UserId.Should().NotBeNullOrWhiteSpace();
		Guid.TryParse(result.UserId, out _).Should().BeTrue("UserId should be a valid GUID string");
	}

	[Fact]
	public void GetAuthorInfos_WithLargeCount_ShouldGenerateSuccessfully()
	{
		// Arrange
		const int count = 100;

		// Act
		List<AuthorInfo> result = FakeAuthorInfo.GetAuthorInfos(count);

		// Assert
		result.Should().HaveCount(count);
		result.Should().OnlyContain(a => !string.IsNullOrWhiteSpace(a.UserId));
	}

	[Fact]
	public void GetNewAuthorInfo_Name_ShouldBeFullName()
	{
		// Arrange & Act
		AuthorInfo result = FakeAuthorInfo.GetNewAuthorInfo();

		// Assert
		result.Name.Should().NotBeNullOrWhiteSpace();
		result.Name.Should().Contain(" ", "name should be a full name with first and last name");
	}
}