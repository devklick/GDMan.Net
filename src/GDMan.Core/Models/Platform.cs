using GDMan.Core.Attributes;

namespace GDMan.Core.Models;

public enum Platform
{
    [Alias("windows", "win", "w")]
    Windows,

    [Alias("macos", "mac", "m")]
    MacOS,

    [Alias("linux", "lin", "l")]
    Linux
}
