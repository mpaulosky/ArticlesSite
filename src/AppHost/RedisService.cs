// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     RedisService.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  AppHost
// =======================================================

using StackExchange.Redis;

namespace AppHost;

/// <summary>
///   Provides extension methods for configuring and managing Redis services within the distributed application.
/// </summary>
public static class RedisServices
{

	/// <summary>
	///   Adds Redis services to the distributed application builder with a clear cache command and persistent lifetime.
	/// </summary>
	/// <param name="builder">The distributed application builder.</param>
	/// <returns>An <see cref="IResourceBuilder{RedisResource}" /> for further configuration.</returns>
	public static IResourceBuilder<RedisResource> AddRedisServices(this IDistributedApplicationBuilder builder)
	{
		IResourceBuilder<RedisResource> cache = builder.AddRedis(RedisCache)
				.WithClearCommand()
				.WithLifetime(ContainerLifetime.Persistent);

		return cache;
	}

	/// <summary>
	///   Adds a custom clear-cache command to the Redis resource builder.
	/// </summary>
	/// <param name="builder">The Redis resource builder.</param>
	/// <returns>The updated <see cref="IResourceBuilder{RedisResource}" />.</returns>
	private static IResourceBuilder<RedisResource> WithClearCommand(this IResourceBuilder<RedisResource> builder)
	{
		builder.WithCommand(
				"clear-cache",
				"Clear Cache",
				context => OnRunClearCacheCommandAsync(builder, context),
				new CommandOptions
				{
						UpdateState = OnUpdateResourceState,
						ConfirmationMessage = "Are you sure you want to clear the cache?",
						Description = "This command will clear all cached data in the Redis cache."
				}
		);

		return builder;
	}

	/// <summary>
	///   Executes the clear-cache command, flushing all data from the Redis cache.
	/// </summary>
	/// <param name="builder">The Redis resource builder.</param>
	/// <param name="context">The command execution context.</param>
	/// <returns>A task representing the asynchronous operation, with the command result.</returns>
	private static async Task<ExecuteCommandResult> OnRunClearCacheCommandAsync(
			IResourceBuilder<RedisResource> builder,
			ExecuteCommandContext context)
	{
		string connectionString = await builder.Resource.GetConnectionStringAsync() ?? throw new InvalidOperationException(
				$"Unable to get the '{context.ResourceName}' connection string.");

		await using ConnectionMultiplexer connection = await ConnectionMultiplexer.ConnectAsync(connectionString);
		IDatabase database = connection.GetDatabase();
		await database.ExecuteAsync("FLUSHALL");

		return CommandResults.Success();
	}

	/// <summary>
	///   Updates the state of the clear-cache command based on the health status of the resource.
	/// </summary>
	/// <param name="context">The update command state context.</param>
	/// <returns>The new <see cref="ResourceCommandState" /> for the command.</returns>
	private static ResourceCommandState OnUpdateResourceState(UpdateCommandStateContext context)
	{
		ILogger<Program> logger = context.ServiceProvider.GetRequiredService<ILogger<Program>>();

		if (logger.IsEnabled(LogLevel.Information))
		{
			logger.LogInformation("Updating resource state: {ResourceSnapshot}", context.ResourceSnapshot);
		}

		return context.ResourceSnapshot.HealthStatus is HealthStatus.Healthy
				? ResourceCommandState.Enabled
				: ResourceCommandState.Disabled;
	}

}