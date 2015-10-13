using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Voyagers.Utilities.RandomDiacritcalMarksGenerator.Utilities
{
    internal class TextElementEnumerable : IEnumerable<string>
    {
        private readonly TextElementEnumerator _enumerator;

        public TextElementEnumerable(string inputString)
        {
            _enumerator = StringInfo.GetTextElementEnumerator(inputString);
        }

        public IEnumerator<string> GetEnumerator()
        {
            while (_enumerator.MoveNext())
            {
                yield return _enumerator.GetTextElement();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
