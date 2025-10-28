using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System.Security.Claims;
using System.Text.Encodings.Web;

using Microsoft.Extensions.Logging;

namespace Web.Tests.Integration.Infrastructure
{
	// Simple Auth0 mock for integration tests
	public class Auth0TestFixture
	{

		public static void AddTestAuth(IServiceCollection services)
		{
			services.AddAuthentication(options =>
					{
						options.DefaultAuthenticateScheme = "TestAuth";
						options.DefaultChallengeScheme = "TestAuth";
					})
					.AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestAuth", options => { });
		}

		private class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
		{

			public TestAuthHandler(
					IOptionsMonitor<AuthenticationSchemeOptions> options,
					ILoggerFactory logger,
					UrlEncoder encoder,
					AuthenticationSchemeOptions schemeOptions)
					: base(options, logger, encoder, schemeOptions.TimeProvider ?? TimeProvider.System) { }

			protected override Task<AuthenticateResult> HandleAuthenticateAsync()
			{
				var claims = new[] { new Claim(ClaimTypes.Name, "TestUser"), new Claim("sub", "auth0|testuser") };
				var identity = new ClaimsIdentity(claims, "TestAuth");
				var principal = new ClaimsPrincipal(identity);
				var ticket = new AuthenticationTicket(principal, "TestAuth");

				return Task.FromResult(AuthenticateResult.Success(ticket));
			}

		}

	}
}
