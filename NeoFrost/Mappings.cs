using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using CloudX.Shared;
using NeoFrost.Types;
using NeoFrost.Types.Conversion;

namespace NeoFrost;

public static class Mappings
{
    private static readonly Dictionary<string, string> Endpoints = new()
    {
        { "stats/onlineUserStats", "stats/onlineStats" },
    };
    
    private static readonly Dictionary<string, string> Replacements = new()
    {
        { "api/", string.Empty },
        { "G-Neos", "G-Resonite" },
        { "Neos%20Essentials", "Resonite%20Essentials" },
    };

    private static readonly List<Type> MappableTypes =
    [
        typeof(ResoniteUserSession),
        typeof(ResoniteContact),
    ];

    public static readonly Dictionary<Type, Type> NeosToResonite = [];
    public static IReadOnlyCollection<Type> NeosTypes => NeosToResonite.Keys;

    [Pure]
    private static IResonite CreateResoniteObject(Type type)
    {
        return (IResonite)Activator.CreateInstance(type);
    }
    
    static Mappings()
    {
        foreach (Type resoType in MappableTypes)
        {
            IResonite reso = CreateResoniteObject(resoType);
            NeosToResonite[reso.NeosType] = resoType;
        }
    }

    public static void MapResource(ref string resource)
    {
        foreach (KeyValuePair<string, string> kvp in Replacements)
        {
            resource = resource.Replace(kvp.Key, kvp.Value);
        }

        foreach (KeyValuePair<string, string> kvp in Endpoints)
        {
            if (resource != kvp.Key) continue;

            resource = kvp.Value;
            break;
        }
    }

    public static object MapObjectToNeos(object obj)
    {
        Debug.Assert(obj != null);
        
        if (obj is IResonite reso)
        {
            Debug.Assert(MappableTypes.Contains(reso.GetType()));
            return reso.ToNeos();
        }
        
        return obj;
    }

    public static object MapObjectToResonite(object obj)
    {
        if (NeosToResonite.TryGetValue(obj.GetType(), out Type resoType))
        {
            IResonite reso = CreateResoniteObject(resoType);
            reso.FromNeos(obj);
            return reso;
        }

        return obj;
    }
}