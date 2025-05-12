using System;

namespace NeoFrost.Types.Conversion;

[AttributeUsage(AttributeTargets.Class)]
public class NeosTypeAttribute : Attribute
{
    public readonly Type NeosType;

    public NeosTypeAttribute(Type neosType)
    {
        NeosType = neosType;
    }
}