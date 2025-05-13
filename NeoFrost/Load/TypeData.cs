using System;

namespace NeoFrost.Load;

public readonly struct TypeData
{
    public TypeData(Type? type, string typeName)
    {
        this.Type = type;
        this.TypeName = typeName;
    }
    
    public readonly Type? Type;
    public readonly string TypeName;

    public override string ToString() => $"TypeData {TypeName} t:'{Type?.ToString() ?? "<not found>"}'";
}