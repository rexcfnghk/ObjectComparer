using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voyagers.Utilities.ObjectComparer.Tests.TestClasses
{
    internal class ClassWithTwoKeys
    {
        [Key]
        public int Id { get; set; }

        [Key]
        public string Name { get; set; }
    }
}
