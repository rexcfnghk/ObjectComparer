using System.Collections.Generic;

namespace Voyagers.Utilities.ObjectComparer.Tests.TestClasses
{
    class CollectionClass
    {
        public IEnumerable<int> Ints { get; set; }

        public IEnumerable<ImmutableClass> ImmutableClasses { get; set; }

        public IEnumerable<User> Users { get; set; } 
    }
}
