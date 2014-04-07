using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voyagers.Utilities.ObjectComparer.Tests.TestClasses
{
    public class Role
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public User User { get; set; }
    }
}
