using System;
using System.Collections.Generic;
using BaseX;

namespace NeoFrost.Load;

public class SlotExtraInfo
{
    public readonly DataTreeNode Node;
    public readonly Dictionary<string, int> FeatureFlags;
    public readonly List<TypeData> Types = [];
    public readonly Dictionary<Type, int> TypeVersions = [];

    public readonly bool LegacyTypes;

    public SlotExtraInfo(DataTreeNode node, DataTreeDictionary featureFlags)
    {
        Node = node;
        FeatureFlags = new Dictionary<string, int>(featureFlags.Children.Count);
        foreach (KeyValuePair<string, DataTreeNode> kvp in featureFlags.Children)
        {
            this.FeatureFlags.Add(kvp.Key, kvp.Value.LoadInt());
        }
        
        this.LegacyTypes = !this.HasFeatureFlag("TypeManagement");
    }

    public int? GetFeatureFlag(string feature)
    {
        if (FeatureFlags.TryGetValue(feature, out int version))
            return version;

        return null;
    }

    public bool HasFeatureFlag(string feature)
    {
        return FeatureFlags.ContainsKey(feature);
    }
    
    public bool HasFeatureFlag(string feature, int version)
    {
        return FeatureFlags.TryGetValue(feature, out int featureVersion) && featureVersion == version;
    }
}