using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voyagers.Utilities.RandomDiacriticalMarksGenerator.Tests.Utilities;
using Xunit;

namespace Voyagers.Utilities.RandomDiacriticalMarksGenerator.Tests
{
    public class TextElementEnumerableTests
    {
        [Fact]
        public void TextElementEnumerable_ValidInput_IsEnumerable()
        {
            const string test = "Foo";

            var result = test.ToTextElements().ToList();

            Assert.Equal(3, result.Count);
            Assert.Equal("F", result[0]);
            Assert.Equal("o", result[1]);
            Assert.Equal("o", result[2]);
        }

        [Fact]
        public void TextElementEnumerable_InputWithDiacritics_ReturnsCorrectCount()
        {
            const string test = "f̸̧̨̧̠͍͙̣̻̖̣̩̼ͯ̏ͦ̈́̊̇́ͅọ̧̨̼͍̬̮̻͗̈́͌͢ͅơ̸̴̶̡̛͉͖̦̹̰̘̻̙̬͇͎̰̌̀ͭ͋ͨͨ͋̈́̃̃̑̔́͑ͬ̀̕͝͡";

            var result = test.ToTextElements().ToList();

            Assert.Equal(3, result.Count);
        }
    }
}
