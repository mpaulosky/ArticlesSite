using Microsoft.Extensions.Options;

namespace Web.Infrastructure;

public class PolicyDiRegistrationTests
{
    [Fact]
    public void IAsyncPolicy_ResultArticle_IsResolvable_FromDI()
    {
        // Arrange
        var services = new ServiceCollection();

        services.Configure<ConcurrencyOptions>(opts =>
        {
            opts.MaxRetries = 3;
            opts.BaseDelayMilliseconds = 100;
            opts.MaxDelayMilliseconds = 2000;
            opts.JitterMilliseconds = 50;
        });

        services.AddSingleton<IAsyncPolicy<Result<Article>>>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ConcurrencyOptions>>().Value;
            return ConcurrencyPolicies.CreatePolicy(options);
        });

        var provider = services.BuildServiceProvider();

        // Act
        var policy = provider.GetService<IAsyncPolicy<Result<Article>>>();
        var policy2 = provider.GetService<IAsyncPolicy<Result<Article>>>();

        // Assert
        policy.Should().NotBeNull();
        policy.Should().BeSameAs(policy2);
    }
}
