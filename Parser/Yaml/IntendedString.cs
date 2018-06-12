using System.Collections.Generic;
using System.Linq;

namespace ResXSemanticParser.Yaml
{
    internal static class IntendedString
    {
        private static readonly Dictionary<int, string> Map = new Dictionary<int, string>();

        public static string From(int intendation)
        {
            if (!Map.TryGetValue(intendation, out var result))
            {
                result = new string(Enumerable.Repeat(' ', intendation).ToArray());
                Map[intendation] = result;
            }

            return result;
        }
    }
}