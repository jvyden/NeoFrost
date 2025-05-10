namespace NeoFrost.Extensions;

internal static class StringExtensions
{
    internal static bool Contains(this string str, char c)
    {
        // ReSharper disable once ForCanBeConvertedToForeach
        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var i = 0; i < str.Length; i++)
        {
            var cStr = str[i];
            if (cStr == c)
                return true;
        }

        return false;
    }
}