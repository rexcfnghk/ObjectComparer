using System;
using System.Globalization;
using Voyagers.Utilities.RandomDiacritcalMarksGenerator;
using Xunit;

namespace Voyagers.Utilities.RandomDiacriticalMarksGenerator.Tests
{
    public class RandomDiacriticalMarksGeneratorTests
    {
        [Fact]
        public void RandomDiacriticalMarksGenerator_InputNull_Throws()
        {
            const string nullString = null;

            var exception = Record.Exception(() => nullString.Abuse(5));

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void RandomDiacriticalMarksGenerator_RandomString_LengthLongerThanOriginal()
        {
            const string testString = "foo";

            var result = testString.Abuse(5);

            Assert.True(testString.Length < result.Length);
        }

        [Fact]
        public void RandomDiacriticalMarksGenerator_RandomString_TextElementLengthTheSameAsOriginal()
        {
            const string testString = "foo";

            var result = testString.Abuse(50);

            Assert.True(new StringInfo(testString).LengthInTextElements ==  new StringInfo(result).LengthInTextElements);
        }

        [Fact]
        public void RandomDiacriticalMarksGenerator_CharactersOutsideBmp_StillWorks()
        {
            const string test = "♩";

            var result = test.Abuse(10);

            Assert.True(new StringInfo(test).LengthInTextElements == new StringInfo(result).LengthInTextElements);
        }
    }
}
