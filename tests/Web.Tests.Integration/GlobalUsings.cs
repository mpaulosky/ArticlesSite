// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GlobalUsings.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Integration
// =======================================================

global using System;
global using System.Collections.Generic;
global using System.Diagnostics.CodeAnalysis;
global using System.Linq;
global using System.Net.Http;
global using System.Security.Claims;
global using System.Threading.Tasks;

global using FluentAssertions;

global using FluentValidation;

global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;

global using MongoDB.Bson;
global using MongoDB.Driver;

global using NSubstitute;

global using Shared.Abstractions;
global using static Shared.Constants.Constants;

global using Testcontainers.MongoDb;

global using Web.Components.Features.Articles.ArticleCreate;
global using Web.Components.Features.Articles.ArticlesList;
global using Web.Components.Features.Articles.Entities;
global using Web.Components.Features.Articles.Fakes;
global using Web.Components.Features.Articles.Interfaces;
global using Web.Components.Features.Articles.Models;
global using Web.Components.Features.Articles.Validators;
global using Web.Components.Features.AuthorInfo.Entities;
global using Web.Components.Features.AuthorInfo.Fakes;
global using Web.Components.Features.Categories.CategoryCreate;
global using Web.Components.Features.Categories.CategoryEdit;
global using Web.Components.Features.Categories.Entities;
global using Web.Components.Features.Categories.Fakes;
global using Web.Components.Features.Categories.Interfaces;
global using Web.Components.Features.Categories.Models;
global using Web.Components.Features.Categories.Validators;
global using Web.Data;
global using Web.Data.Repositories;
global using Web.Tests.Integration.Infrastructure;

global using Xunit;
