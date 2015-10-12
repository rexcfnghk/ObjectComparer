using System;
using System.Globalization;
using System.Text;

namespace Voyagers.Utilities.RandomDiacritcalMarksGenerator
{
    public static class RandomDiacritcalMarksGenerator
    {
        // Combining Diacritcal Marks are from U+0300 to U+036F
        private const int _rangeLowest = 0x0300;
        private const int _rangeHighest = 0x036F;
        private static readonly Random _random = new Random();

        public static string Abuse(this string input, int maxDiacritcalMarksPerGraphemeCluster)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var stringBuilder = new StringBuilder();
            TextElementEnumerator textElementEnumerator = StringInfo.GetTextElementEnumerator(input);
            while (textElementEnumerator.MoveNext())
            {
                int numberOfDiacriticalMarks = _random.Next(-1, maxDiacritcalMarksPerGraphemeCluster) + 1;
                stringBuilder.Append(AddCombiningDiacritics(textElementEnumerator.GetTextElement(),
                                                            numberOfDiacriticalMarks));
            }

            return stringBuilder.ToString();
        }

        private static string AddCombiningDiacritics(string input, int number)
        {
            var stringBuilder = new StringBuilder(input, input.Length + number);
            for (int i = 0; i < number; i++)
            {
                stringBuilder.Append(GenerateRandomCombiningDiacritcalMark());
            }

            return stringBuilder.ToString();
        }

        private static string GenerateRandomCombiningDiacritcalMark()
            => char.ConvertFromUtf32(_random.Next(_rangeLowest - 1, _rangeHighest) + 1);
    }
}
