namespace Voyagers.Utilities.ObjectComparer.Tests.TestClasses
{
    public class ImmutableClass
    {
        private readonly int _int1;
        private readonly string _string1;

        public ImmutableClass(int int1, string string1)
        {
            _int1 = int1;
            _string1 = string1;
        }

        public int Int1
        {
            get { return _int1; }
        }

        public string String1
        {
            get { return _string1; }
        }
    }
}
