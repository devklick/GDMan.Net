using System.Reflection;

namespace GDMan.Core.Models.Interfaces;

public interface IFauxEnum
{
    public string Identifier { get; }
    public abstract IReadOnlyCollection<string> Aliases { get; }
    public static abstract IEnumerable<PropertyInfo> GetValues();
}