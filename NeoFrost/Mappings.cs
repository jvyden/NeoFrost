using System.Collections.Generic;

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
        
        if (resource.StartsWith("users/") && resource.EndsWith("/friends"))
            resource = resource.Replace("/friends", "/contacts");
    }
}