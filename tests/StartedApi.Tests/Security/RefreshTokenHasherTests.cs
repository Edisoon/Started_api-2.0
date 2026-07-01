using FluentAssertions;
using StartedApi.Infrastructure.Security;

namespace StartedApi.Tests.Security;

public sealed class RefreshTokenHasherTests
{
    [Fact]
    public void Hash_ReturnsSameHash_ForSameToken()
    {
        var hasher = new RefreshTokenHasher();

        var first = hasher.Hash("token-value");
        var second = hasher.Hash("token-value");

        first.Should().Be(second);
        first.Should().NotBe("token-value");
        first.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Hash_ReturnsDifferentHash_ForDifferentTokens()
    {
        var hasher = new RefreshTokenHasher();

        var first = hasher.Hash("token-value");
        var second = hasher.Hash("other-token-value");

        first.Should().NotBe(second);
    }
}
