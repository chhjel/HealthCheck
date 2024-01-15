using QoDL.Toolkit.Core.Util;
using System;
using Xunit;
using Xunit.Abstractions;

namespace QoDL.Toolkit.Core.Tests.Utils;

public class TKIPAddressUtilsTests
{
    public ITestOutputHelper Output { get; }

    public TKIPAddressUtilsTests(ITestOutputHelper output)
    {
        Output = output;
    }

    [Theory]
    [InlineData("10.20.30.40", "10.20.30.0/0")]
    [InlineData("10.20.30.40", "10.20.30.0/1")]
    [InlineData("10.20.30.40", "10.20.30.0/5")]
    [InlineData("10.20.30.40", "10.20.30.0/10")]
    [InlineData("10.20.30.40", "10.20.30.0/24")]
    [InlineData("10.20.30.40", "10.20.30.0/32")]
    [InlineData("10.20.30.40", "10.20.30.40/5")]
    public void IpMatchesOrIsWithinCidrRange_WithValidRanges_DoesNotThrow(string ip, string ipWithOptionalCidr)
    {
        TKIPAddressUtils.IpMatchesOrIsWithinCidrRange(ip, ipWithOptionalCidr);
    }

    [Theory]
    [InlineData("localhost", "10.20.30.0/0")]
    [InlineData("asd", "10.20.30.0/0")]
    [InlineData("a.b.c.d", "10.20.30.0/0")]
    public void IpMatchesOrIsWithinCidrRange_WithInvalidIp_Throws(string ip, string ipWithOptionalCidr)
    {
        Assert.Throws<ArgumentException>(() =>
        {
            TKIPAddressUtils.IpMatchesOrIsWithinCidrRange(ip, ipWithOptionalCidr);
        });
    }
}
