using System.Collections.Generic;

namespace Voyagers.Utilities.RandomDiacriticalMarksGenerator.Tests.Utilities
{
    public static class TextElementEnumerableUtilities
    {
        public static IEnumerable<string> ToTextElements(this string input) => new TextElementEnumerable(input);
    }
}
