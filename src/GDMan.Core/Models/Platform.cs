using GDMan.Core.Attributes;

namespace GDMan.Core.Models;

public enum Platform
{
    [Alias("win", "w")]
    Windows,

    [Alias("lin", "l")]
    Linux,

    [Alias("mac", "osx", "m")]
    MacOS
}