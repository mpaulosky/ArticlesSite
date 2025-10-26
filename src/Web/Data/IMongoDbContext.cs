// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     IMongoDbContext.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web
// =======================================================

namespace Web.Data;

/// <summary>
/// Interface for MongoDB context using a native driver
/// </summary>
public interface IMongoDbContext
{
	IMongoCollection<Article> Articles { get; }
	IMongoCollection<Category> Categories { get; }
	IMongoDatabase Database { get; }
}