using Web.Components.User;

namespace Web.Tests.Unit.Components.User;

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
		var value = typeof(Profile).GetMethod("GetClaimValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
				.Invoke(null, new object[] { principal, new[] { ClaimTypes.Email } });
		Assert.Equal("test@example.com", value);
	}
}
