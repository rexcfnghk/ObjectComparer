using Voyagers.Utilities.ObjectComparer.Attributes;

namespace Voyagers.Utilities.ObjectComparer.Tests.TestClasses
{
    [IgnoreVariance]
    public class IgnoreVarianceClass
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool IsHappy { get; set; }
    }
}
