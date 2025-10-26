// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GlobalUsings.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

#region

global using MongoDB.Bson;

global using Shared.Abstractions;
global using Shared.Entities;
global using Shared.Interfaces;
global using Shared.Models;

global using static Shared.Abstractions.Result<Shared.Models.ArticleDto>;

global using MongoDB.Bson;
global using MongoDB.Driver;

global using Shared.Abstractions;
global using Shared.Entities;
global using Shared.Interfaces;
global using Shared.Models;

global using Web.Components;
global using Web.Components.Features.Articles.ArticleCreate;
global using Web.Components.Features.Articles.ArticleDetails;
global using Web.Components.Features.Articles.ArticleEdit;
global using Web.Components.Features.Articles.ArticlesList;
global using Web.Components.Features.Categories.CategoriesList;
global using Web.Components.Features.Categories.CategoryCreate;
global using Web.Components.Features.Categories.CategoryDetails;
global using Web.Components.Features.Categories.CategoryEdit;
global using Web.Data;
global using Web.Data.Repositories;
global using Web.Services;

global using static Shared.Constants.Constants;

global using static Web.Components.Features.Articles.ArticleCreate.CreateArticle;
global using static Web.Components.Features.Articles.ArticleDetails.GetArticle;
global using static Web.Components.Features.Articles.ArticleEdit.EditArticle;
global using static Web.Components.Features.Articles.ArticlesList.GetArticles;
global using static Web.Components.Features.Categories.CategoriesList.GetCategories;
global using static Web.Components.Features.Categories.CategoryCreate.CreateCategory;
global using static Web.Components.Features.Categories.CategoryDetails.GetCategory;
global using static Web.Components.Features.Categories.CategoryEdit.EditCategory;

#endregion
