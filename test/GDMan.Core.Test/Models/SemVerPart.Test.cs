using FluentAssertions;

using GDMan.Core.Models;

namespace GDMan.Core.Test.Models;

public class SemVerPartTest
{
    [Theory]
    [InlineData(SemVerPartType.Major, "alpha")]
    [InlineData(SemVerPartType.Minor, "()_+")]
    [InlineData(SemVerPartType.Patch, "-1")]
    [InlineData(SemVerPartType.Patch, "")]
    public void Ctor_NonSuffix_ThrowsWhenValueInvalid(SemVerPartType type, string value)
    {
        var act = () => new SemVerPart(type, value);
        act.Should().Throw<FormatException>().WithMessage($"Invalid {type} version part");
    }

    [Theory]
    [InlineData("1")]
    [InlineData("0")]
    public void Ctor_ShouldParseNumericValue(string value)
    {
        var sut = new SemVerPart(SemVerPartType.Major, value);
        sut.NumericValue.Should().Be(int.Parse(value));
    }

    [Fact]
    public void Ctor_ShouldThrowWhenNegative()
    {
        var type = SemVerPartType.Major;
        var act = () => new SemVerPart(type, "-1");
        act.Should().Throw<FormatException>().WithMessage($"Invalid {type} version part");
    }

    [Theory]
    [InlineData("()")]
    [InlineData("")]
    [InlineData(" ")]
    public void Ctor_ShouldThrowWhenInvalidSuffix(string value)
    {
        var type = SemVerPartType.Suffix;
        var act = () => new SemVerPart(type, value);
        act.Should().Throw<FormatException>().WithMessage($"Invalid {type} version part");
    }

    [Fact]
    public void Ctor_ShouldUseValueTypeWhenNumeric()
    {
        var value = "123";
        var sut = new SemVerPart(SemVerPartType.Major, value);
        sut.ValueType.Should().Be(SemVerValueType.Absolute);
    }

    [Fact]
    public void Ctor_ShouldUseValueTypeWhenValidSuffix()
    {
        var value = "alpha";
        var sut = new SemVerPart(SemVerPartType.Suffix, value);
        sut.ValueType.Should().Be(SemVerValueType.Absolute);
    }

    [Fact]
    public void Ctor_ShouldUseWildcardWhenAsterisk()
    {
        var value = "*";
        var sut = new SemVerPart(SemVerPartType.Major, value);
        sut.ValueType.Should().Be(SemVerValueType.Wildcard);
    }

    [Fact]
    public void IsMatch_FalseWhenDifferentType()
    {
        var one = new SemVerPart(SemVerPartType.Major, "1");
        var two = new SemVerPart(SemVerPartType.Minor, "1");
        one.IsMatch(two).Should().BeFalse();
        two.IsMatch(one).Should().BeFalse();
    }

    [Theory]
    [InlineData("1", "*")]
    [InlineData("*", "1")]
    public void IsMatch_TrueWhenEitherIsWildcard(string oneStr, string twoStr)
    {
        var one = new SemVerPart(SemVerPartType.Major, oneStr);
        var two = new SemVerPart(SemVerPartType.Major, twoStr);
        one.IsMatch(two).Should().BeTrue();
        two.IsMatch(one).Should().BeTrue();
    }
}
