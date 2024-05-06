using FluentAssertions;

using GDMan.Core.Helpers;

namespace GDMan.Core.Test.Helpers;

public class SemVerHelperTest
{
    #region TryParseVersion
    [Theory]
    [InlineData("1", true, "1.0.0")]
    [InlineData("1.2", true, "1.2.0")]
    [InlineData("1.2-stable", true, "1.2.0-stable")]
    [InlineData("1.2.3", true, "1.2.3")]
    [InlineData("1.2.3-stable", true, "1.2.3-stable")]
    [InlineData("a", false, null)]
    [InlineData("a.b", false, null)]
    [InlineData("a.b-stable", false, null)]
    [InlineData("a.b.c", false, null)]
    [InlineData("a.b.c-stable", false, null)]
    public void TryParseVersion(string input, bool expectValid, string? expectExactVersion)
    {
        var result = SemVerHelper.TryParseVersion(input, out var version);
        result.Should().Be(expectValid);

        if (expectValid)
        {
            version.Should().Be(new SemanticVersioning.Version(expectExactVersion));
        }
        else
        {
            version.Should().BeNull();
        }
    }
    #endregion
}