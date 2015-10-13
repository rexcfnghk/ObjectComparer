using System.Collections.Generic;

namespace Voyagers.Utilities.RandomDiacritcalMarksGenerator.Utilities
{
    public static class TextElementEnumerableUtilities
    {
        public static IEnumerable<string> ToTextElements(this string input) => new TextElementEnumerable(input);
    }
}
