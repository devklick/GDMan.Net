using System.ComponentModel;

namespace GDMan.Core.Models;

public enum Flavour
{
    [Description("The standard version of Godot which uses GDScript")]
    Standard,

    [Description("The version of Godot required to use .Net C#")]
    Mono
}