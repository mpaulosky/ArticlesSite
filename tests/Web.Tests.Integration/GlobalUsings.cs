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
global using System.Threading.Tasks;

global using FluentAssertions;

global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;

global using MongoDB.Bson;
global using MongoDB.Driver;

global using NSubstitute;

global using Shared.Abstractions;
global using Shared.Entities;
global using Shared.Fakes;
global using Shared.Interfaces;
global using Shared.Models;

global using Testcontainers.MongoDb;

global using Web.Data;
global using Web.Data.Repositories;
global using Web.Tests.Integration.Infrastructure;

global using FluentValidation;

global using Xunit;
