using Keycloak.AdminSdk.Common;

namespace Keycloak.AdminSdk.UnitTests.Common;

public sealed class KeycloakResourceIdTests
{
    [Fact]
    public void ConstructorWithValidValuePreservesValue()
    {
        var resourceId = new KeycloakResourceId("55de43bb-55db-46e9-a537-b220282f2457");

        Assert.Equal("55de43bb-55db-46e9-a537-b220282f2457", resourceId.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void ConstructorWithInvalidValueThrows(string value)
    {
        Assert.Throws<ArgumentException>(() => new KeycloakResourceId(value));
    }
}
