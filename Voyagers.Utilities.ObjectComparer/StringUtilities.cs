using System.Text.RegularExpressions;

namespace Voyagers.Utilities.ObjectComparer
{
    public static class StringUtilities
    {
        public static string TrimExtraSpacesBetweenWords(this string input)
        {
            return Regex.Replace(input, @"\s+", " ");
        }
    }
}
