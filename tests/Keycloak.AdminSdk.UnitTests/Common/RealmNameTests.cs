using Keycloak.AdminSdk.Common;

namespace Keycloak.AdminSdk.UnitTests.Common;

public sealed class RealmNameTests
{
    [Fact]
    public void ConstructorWithValidValuePreservesValue()
    {
        var realm = new RealmName("customers");

        Assert.Equal("customers", realm.Value);
        Assert.Equal("customers", realm.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void ConstructorWithInvalidValueThrows(string value)
    {
        Assert.Throws<ArgumentException>(() => new RealmName(value));
    }

    [Fact]
    public void ImplicitConversionsRoundTripValue()
    {
        RealmName realm = "customers";
        string value = realm;

        Assert.Equal("customers", value);
    }
}
