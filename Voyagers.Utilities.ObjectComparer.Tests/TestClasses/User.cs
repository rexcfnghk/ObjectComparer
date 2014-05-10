using System.ComponentModel.DataAnnotations;
using Voyagers.Utilities.ObjectComparer.Attributes;

namespace Voyagers.Utilities.ObjectComparer.Tests.TestClasses
{
    internal class User
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        [IgnoreVariance]
        public int Age { get; set; }

        public Role Role { get; set; }
    }
}
