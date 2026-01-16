// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CategoryEditComponentTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Unit
// =======================================================

namespace Web.Tests.Unit.Components.Features.Categories;

[ExcludeFromCodeCoverage]
public class CategoryEditComponentTests : BunitContext
{
	//[Fact]
	//public void RendersLoadingState()
	//{
	//    var handler = Substitute.For<CategoryDetails.GetCategory.IGetCategoryHandler>();
	//    handler.HandleAsync(Arg.Any<object>()).Returns(x => { Task.Delay(100); return Result.Fail<CategoryDto>("Loading..."); });
	//    Services.AddSingleton(handler);
	//    var cut = Render<Edit>();
	//    cut.Markup.Contains("Loading");
	//}

	//[Fact]
	//public void RendersErrorState()
	//{
	//    var handler = Substitute.For<CategoryDetails.GetCategory.IGetCategoryHandler>();
	//    handler.HandleAsync(Arg.Any<object>()).Returns(Result.Fail<CategoryDto>("Test error"));
	//    Services.AddSingleton(handler);
	//    var cut = Render<Edit>();
	//    cut.WaitForAssertion(() =>
	//    {
	//        cut.Markup.Contains("Unable to load category");
	//        cut.Markup.Contains("Test error");
	//    });
	//}

	//[Fact]
	//public void RendersFormWithCategory()
	//{
	//    var category = new CategoryDto { CategoryName = "TestCat" };
	//    var handler = Substitute.For<CategoryDetails.GetCategory.IGetCategoryHandler>();
	//    handler.HandleAsync(Arg.Any<object>()).Returns(Result.Ok(category));
	//    Services.AddSingleton(handler);
	//    var cut = Render<Edit>();
	//    cut.WaitForAssertion(() =>
	//    {
	//        cut.Markup.Contains("TestCat");
	//        cut.Find("input#categoryName").Attributes["value"].Value.Should().Be("TestCat");
	//    });
	//}
}