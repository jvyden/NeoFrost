using System.Collections.Generic;
using BaseX;

namespace NeoFrost.Extensions;

public static class DataTreeDictionaryExtensions
{
    public static bool Contains(this DataTreeDictionary root, DataTreeNode node)
    {
        if (root.Children.ContainsValue(node))
            return true;

        foreach (DataTreeNode subNode in root.Children.Values)
            if(subNode.Contains(node))
                return true;

        return false;
    }
    
    public static bool Contains(this DataTreeList root, DataTreeNode node)
    {
        if (root.Children.Contains(node))
            return true;
        
        foreach (DataTreeNode subNode in root.Children)
            if(subNode.Contains(node))
                return true;

        return false;
    }

    public static bool Contains(this DataTreeNode root, DataTreeNode node)
    {
        if (root is DataTreeDictionary dict)
            return dict.Contains(node);

        if (root is DataTreeList list)
            return list.Contains(node);

        return false;
    }
}