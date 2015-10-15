using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Voyagers.Utilities.RandomDiacritcalMarksGenerator.Utilities
{
    internal class TextElementEnumerable : IEnumerable<string>
    {
        private readonly TextElementEnumerator _enumerator;

        public TextElementEnumerable(string inputString) : this(StringInfo.GetTextElementEnumerator(inputString))
        {
        }

        public TextElementEnumerable(TextElementEnumerator enumerator)
        {
            _enumerator = enumerator;
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
