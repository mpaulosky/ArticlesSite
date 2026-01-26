using Web.Components.User;

namespace Web.Components.Features.UserInfo;

[ExcludeFromCodeCoverage]
public class ProfileTests
{
	[Fact]
	public void GetClaimValue_Should_Return_Claim()
	{
		var claims = new[] {
						new Claim(ClaimTypes.Name, "TestUser"),
						new Claim(ClaimTypes.Email, "test@example.com")
				};
		var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
		var value = typeof(Profile).GetMethod("GetClaimValue", BindingFlags.NonPublic | BindingFlags.Static)!
				.Invoke(null, [ principal, new[] { ClaimTypes.Email } ]);
		Assert.Equal("test@example.com", value);
	}
}
