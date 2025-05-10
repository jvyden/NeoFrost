using System;

namespace NeoFrost.Types.Conversion;

public interface IResonite
{
    public object ToNeos();
    public void FromNeos(object original);
    public Type NeosType { get; }
}