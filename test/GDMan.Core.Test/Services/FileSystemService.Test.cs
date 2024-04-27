using FluentAssertions;

using GDMan.Core.Infrastructure;
using GDMan.Core.Models;
using GDMan.Core.Services.FileSystem;

using Moq;

using Semver;

namespace GDMan.Core.Test.Services;

public class FileSystemServiceTest
{
    private readonly FS _fs;
    public FileSystemServiceTest()
    {
        var paths = new KnownPaths();
        var logger = new ConsoleLogger();
        _fs = new FS(
            logger,
            new GDManBinDirectory(paths, logger),
            new GDManVersionsDirectory(paths, logger)
        );
    }
    #region Linux
    [Fact]
    public void GenerateName_Linux_Arm32_Standard()
    {
        var version = SemVersion.Parse("4.2.1-stable", SemVersionStyles.Any);
        var platform = Platform.Linux;
        var architecture = Architecture.Arm32;
        var flavour = Flavour.Standard;
        var expected = "Godot_v4.2.1-stable_linux.arm32";

        var result = _fs.GenerateVersionName(version, platform, architecture, flavour);

        result.Should().Be(expected);
    }

    [Fact]
    public void GenerateName_Linux_Arm32_Mono()
    {
        var version = SemVersion.Parse("4.2-stable", SemVersionStyles.Any);
        var platform = Platform.Linux;
        var architecture = Architecture.Arm32;
        var flavour = Flavour.Mono;
        var expected = "Godot_v4.2.0-stable_mono_linux_arm32";

        var result = _fs.GenerateVersionName(version, platform, architecture, flavour);

        result.Should().Be(expected);
    }

    [Fact]
    public void GenerateName_Linux_Arm64_Standard()
    {
        var version = SemVersion.Parse("3.9.9-alpha", SemVersionStyles.Any);
        var platform = Platform.Linux;
        var architecture = Architecture.Arm64;
        var flavour = Flavour.Standard;
        var expected = "Godot_v3.9.9-alpha_linux.arm64";

        var result = _fs.GenerateVersionName(version, platform, architecture, flavour);

        result.Should().Be(expected);
    }

    [Fact]
    public void GenerateName_Linux_Arm64_Mono()
    {
        var version = SemVersion.Parse("3.5.3-alpha", SemVersionStyles.Any);
        var platform = Platform.Linux;
        var architecture = Architecture.Arm64;
        var flavour = Flavour.Mono;
        var expected = "Godot_v3.5.3-alpha_mono_linux_arm64";

        var result = _fs.GenerateVersionName(version, platform, architecture, flavour);

        result.Should().Be(expected);
    }

    [Fact]
    public void GenerateName_Linux_X64_Standard()
    {
        var version = SemVersion.Parse("1.2.3", SemVersionStyles.Any);
        var platform = Platform.Linux;
        var architecture = Architecture.X64;
        var flavour = Flavour.Standard;
        var expected = "Godot_v1.2.3_linux.x86_64";

        var result = _fs.GenerateVersionName(version, platform, architecture, flavour);

        result.Should().Be(expected);
    }

    [Fact]
    public void GenerateName_Linux_X64_Mono()
    {
        var version = SemVersion.Parse("1.2.3", SemVersionStyles.Any);
        var platform = Platform.Linux;
        var architecture = Architecture.X64;
        var flavour = Flavour.Mono;
        var expected = "Godot_v1.2.3_mono_linux_x86_64";

        var result = _fs.GenerateVersionName(version, platform, architecture, flavour);

        result.Should().Be(expected);
    }

    [Fact]
    public void GenerateName_Linux_X86_Standard()
    {
        var version = SemVersion.Parse("1.2.3", SemVersionStyles.Any);
        var platform = Platform.Linux;
        var architecture = Architecture.X86;
        var flavour = Flavour.Standard;
        var expected = "Godot_v1.2.3_linux.x86_32";

        var result = _fs.GenerateVersionName(version, platform, architecture, flavour);

        result.Should().Be(expected);
    }
    #endregion

    #region Windows
    [Fact]
    public void GenerateName_Windows_X86_Standard()
    {
        var version = SemVersion.Parse("1.2.3", SemVersionStyles.Any);
        var platform = Platform.Windows;
        var architecture = Architecture.X86;
        var flavour = Flavour.Standard;
        var expected = "Godot_v1.2.3_win32.exe";

        var result = _fs.GenerateVersionName(version, platform, architecture, flavour);

        result.Should().Be(expected);
    }
    [Fact]
    public void GenerateName_Windows_X86_Mono()
    {
        var version = SemVersion.Parse("1.2.3", SemVersionStyles.Any);
        var platform = Platform.Windows;
        var architecture = Architecture.X86;
        var flavour = Flavour.Mono;
        var expected = "Godot_v1.2.3_mono_win32";

        var result = _fs.GenerateVersionName(version, platform, architecture, flavour);

        result.Should().Be(expected);
    }
    [Fact]
    public void GenerateName_Windows_X64_Standard()
    {
        var version = SemVersion.Parse("1.2.3", SemVersionStyles.Any);
        var platform = Platform.Windows;
        var architecture = Architecture.X64;
        var flavour = Flavour.Standard;
        var expected = "Godot_v1.2.3_win64.exe";

        var result = _fs.GenerateVersionName(version, platform, architecture, flavour);

        result.Should().Be(expected);
    }
    [Fact]
    public void GenerateName_Windows_X64_Mono()
    {
        var version = SemVersion.Parse("1.2.3", SemVersionStyles.Any);
        var platform = Platform.Windows;
        var architecture = Architecture.X64;
        var flavour = Flavour.Mono;
        var expected = "Godot_v1.2.3_mono_win64";

        var result = _fs.GenerateVersionName(version, platform, architecture, flavour);

        result.Should().Be(expected);
    }
    [Fact]
    public void GenerateName_Windows_Arm_Throws()
    {
        var arches = new List<Architecture>([Architecture.Arm32, Architecture.Arm64]);
        var version = SemVersion.Parse("1.2.3", SemVersionStyles.Any);
        var platform = Platform.Windows;
        var flavour = Flavour.Mono;

        foreach (var arch in arches)
        {
            var act = () => _fs.GenerateVersionName(version, platform, arch, flavour);
            act.Should().Throw<InvalidOperationException>().WithMessage($"Architecture {arch} not supported on Windows platform");
        }
    }
    #endregion

    #region MacOS
    [Fact]
    public void GenerateName_MacOS_Standard_ArchIgnored()
    {
        var version = SemVersion.Parse("1.2.3", SemVersionStyles.Any);
        var platform = Platform.MacOS;
        var flavour = Flavour.Standard;
        foreach (var arch in Enum.GetValues(typeof(Architecture)))
        {
            var expected = "Godot_v1.2.3_macos.universal";
            var result = _fs.GenerateVersionName(version, platform, (Architecture)arch, flavour);
            result.Should().Be(expected);
        }
    }
    [Fact]
    public void GenerateName_MacOS_Mono_ArchIgnored()
    {
        var version = SemVersion.Parse("1.2.3", SemVersionStyles.Any);
        var platform = Platform.MacOS;
        var flavour = Flavour.Mono;
        foreach (var arch in Enum.GetValues(typeof(Architecture)))
        {
            var expected = "Godot_v1.2.3_mono_macos.universal";
            var result = _fs.GenerateVersionName(version, platform, (Architecture)arch, flavour);
            result.Should().Be(expected);
        }
    }
    #endregion
}