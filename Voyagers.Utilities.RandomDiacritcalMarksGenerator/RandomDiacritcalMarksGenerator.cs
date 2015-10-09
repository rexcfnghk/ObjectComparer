using System;
using System.Globalization;
using System.Text;

namespace Voyagers.Utilities.RandomDiacritcalMarksGenerator
{
    public static class RandomDiacritcalMarksGenerator
    {
        private const int _rangeLowest = 768;
        private const int _rangeHighest = 879;

        public static string Abuse(this string input, int threshold)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var stringBuilder = new StringBuilder();
            var random = new Random();
            TextElementEnumerator textElementEnumerator = StringInfo.GetTextElementEnumerator(input);
            while (textElementEnumerator.MoveNext())
            {
                int numberOfDiacriticalMarks = random.Next(-1, threshold) + 1;

            }

            throw new NotImplementedException();
        }

        private static string AddCombiningDiacritics(string input, int number)
        {
            var stringBuilder = new StringBuilder(input, input.Length + number);
            var random = new Random();
            for (int i = 0; i < number; i++)
            {
                byte randomDiacrtic = Convert.ToByte(random.Next(_rangeLowest - 1, _rangeHighest) + 1);
                stringBuilder.Append(Encoding.Unicode.GetString(new[] { randomDiacrtic }));
            }

            return stringBuilder.ToString();
        }
    }
}
