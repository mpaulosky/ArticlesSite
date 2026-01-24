// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GlobalUsings.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

global using System.ComponentModel;
global using System.ComponentModel.DataAnnotations;
global using System.Linq.Expressions;

global using Auth0.AspNetCore.Authentication;

global using Bogus;

global using FluentValidation;

global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Authentication.Cookies;
global using Microsoft.AspNetCore.Components.Authorization;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.JSInterop;

global using MongoDB.Bson;
global using MongoDB.Bson.Serialization.Attributes;
global using MongoDB.Driver;

global using Shared.Abstractions;

global using Web.Components;
global using Web.Components.Features.Articles.Entities;
global using Web.Components.Features.Articles.Fakes;
global using Web.Components.Features.Articles.Interfaces;
global using Web.Components.Features.Articles.Models;
// Handler and feature namespaces used across the project
global using Web.Components.Features.Articles.ArticleCreate;
global using Web.Components.Features.Articles.ArticleDetails;
global using Web.Components.Features.Articles.ArticleEdit;
global using Web.Components.Features.Articles.ArticlesList;
global using Web.Components.Features.Categories.CategoriesList;
global using Web.Components.Features.Categories.CategoryCreate;
global using Web.Components.Features.Categories.CategoryDetails;
global using Web.Components.Features.Categories.CategoryEdit;
global using Web.Components.Features.AuthorInfo.Entities;
global using Web.Components.Features.AuthorInfo.Fakes;
global using Web.Components.Features.Categories.Entities;
global using Web.Components.Features.Categories.Fakes;
global using Web.Components.Features.Categories.Interfaces;
global using Web.Components.Features.Categories.Models;
global using Web.Data;
global using Web.Data.Repositories;
global using Web.Services;

// Common system and helper usings for Web project
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;
global using Polly;
global using Polly.Registry;
global using Microsoft.Extensions.Logging;
global using System.IO;
global using System.Security.Claims;
global using System.Diagnostics;
// Infrastructure types
global using Web.Infrastructure;

// Alias for strongly-typed article concurrency policy used in DI and tests
// Provide a public interface type for the strongly-typed policy used in DI and tests
global using IArticleConcurrencyPolicy = Polly.IAsyncPolicy<Shared.Abstractions.Result<Web.Components.Features.Articles.Entities.Article>>;

global using static Shared.Constants.Constants;
global using static Shared.Helpers.Helpers;
