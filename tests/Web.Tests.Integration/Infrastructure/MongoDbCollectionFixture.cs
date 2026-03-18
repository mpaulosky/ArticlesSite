// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     MongoDbCollectionFixture.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Integration
// =======================================================

namespace Web.Tests.Integration.Infrastructure;

/// <summary>
///   xUnit collection definition for MongoDB fixture sharing
/// </summary>
[CollectionDefinition("MongoDb Collection")]
public class MongoDbCollectionFixture : ICollectionFixture<MongoDbFixture>
{
	// This class has no code and is never created. Its purpose is simply
	// to be the place to apply [CollectionDefinition] and all the
	// ICollectionFixture<> interfaces.
}
