using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Web.Infrastructure;
using Xunit;

namespace Web.Tests.Unit.Infrastructure;

public class PolicyRegistrationTests
{
    [Fact]
    public void ArticleConcurrencyPolicy_IsRegistered_InServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Configure ConcurrencyOptions in DI
        services.Configure<ConcurrencyOptions>(opts =>
        {
            opts.MaxRetries = 3;
            opts.BaseDelayMilliseconds = 100;
            opts.MaxDelayMilliseconds = 2000;
            opts.JitterMilliseconds = 50;
        });

        // Register the strongly-typed policy as the app would
        services.AddSingleton<IArticleConcurrencyPolicy>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ConcurrencyOptions>>().Value;
            return ConcurrencyPolicies.CreatePolicy(options);
        });

        var provider = services.BuildServiceProvider();

        // Act
        var policy1 = provider.GetService<IArticleConcurrencyPolicy>();
        var policy2 = provider.GetService<IArticleConcurrencyPolicy>();

        // Assert
        policy1.Should().NotBeNull();
        policy1.Should().BeOfType<IArticleConcurrencyPolicy>();
        // Should be singleton
        policy1.Should().BeSameAs(policy2);
    }
}
