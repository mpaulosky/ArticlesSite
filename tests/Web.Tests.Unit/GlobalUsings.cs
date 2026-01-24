// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GlobalUsings.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Unit
// =======================================================

global using System;
global using System.Collections.Generic;
global using System.Diagnostics.CodeAnalysis;
global using System.Linq;
global using System.Reflection;
global using System.Security.Claims;
global using System.Threading.Tasks;

global using Bunit;

global using FluentAssertions;

global using FluentValidation;
global using FluentValidation.Results;

// Polly policy types used by concurrency tests
global using Polly;
global using Polly.Registry;
global using IArticleConcurrencyPolicy = Polly.IAsyncPolicy<Shared.Abstractions.Result<Web.Components.Features.Articles.Entities.Article>>;

global using Microsoft.AspNetCore.Components.Authorization;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;

global using MongoDB.Bson;

global using NSubstitute;

global using Shared.Abstractions;
// Articles
global using Web.Components.Features.Articles.ArticleCreate;
global using Web.Components.Features.Articles.ArticleDetails;
global using Web.Components.Features.Articles.ArticleEdit;
// Article list/details handlers
global using Web.Components.Features.Articles.ArticlesList;
// Article & Category entities + fakes
global using Web.Components.Features.Articles.Entities;
// Article mapping extensions
global using Web.Components.Features.Articles.Extensions;
global using Web.Components.Features.Articles.Fakes;
global using Web.Components.Features.Articles.Interfaces;
global using Web.Components.Features.Articles.Models;
global using Web.Components.Features.Articles.Validators;
// AuthorInfo
global using Web.Components.Features.AuthorInfo.Entities;
global using Web.Components.Features.AuthorInfo.Extensions;
// Categories
global using Web.Components.Features.Categories.CategoryCreate;
global using Web.Components.Features.Categories.CategoryDetails;
global using Web.Components.Features.Categories.CategoryEdit;
global using Web.Components.Features.Categories.Entities;
global using Web.Components.Features.Categories.Extensions;
global using Web.Components.Features.Categories.Fakes;
global using Web.Components.Features.Categories.Interfaces;
global using Web.Components.Features.Categories.Models;
global using Web.Components.Features.Categories.Validators;
global using Web.Services;

global using Xunit;
// Alias to disambiguate AuthorInfo type (namespace also exists with same name)
global using AuthorInfo = Web.Components.Features.AuthorInfo.Entities.AuthorInfo;
