using System;
using System.Text;
using Voyagers.Utilities.RandomDiacritcalMarksGenerator.Utilities;

namespace Voyagers.Utilities.RandomDiacritcalMarksGenerator
{
    public static class RandomDiacritcalMarksGenerator
    {
        // Combining Diacritcal Marks are from U+0300 to U+036F
        private const int _rangeLowest = 0x0300;
        private const int _rangeHighest = 0x036F;
        private static readonly Random _random = new Random();

        public static string Abuse(this string input, int maxDiacritcalMarksPerGrapheme)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var stringBuilder = new StringBuilder();
            foreach (string textElement in input.ToTextElements())
            {
                int numberOfDiacriticalMarks = GetRandomNumber(0, maxDiacritcalMarksPerGrapheme);
                stringBuilder.Append(AddCombiningDiacritics(textElement, numberOfDiacriticalMarks));
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

        private static int GetRandomNumber(int minValueInclusive = 0, int maxValueInclusive = 1)
            => _random.Next(minValueInclusive - 1, maxValueInclusive) + 1;

        private static string GenerateRandomCombiningDiacritcalMark()
            => char.ConvertFromUtf32(GetRandomNumber(_rangeLowest, _rangeHighest));
    }
}
