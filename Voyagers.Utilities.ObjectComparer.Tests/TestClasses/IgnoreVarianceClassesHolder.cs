using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voyagers.Utilities.ObjectComparer.Tests.TestClasses
{
    internal class IgnoreVarianceClassesHolder
    {
        public IgnoreVarianceClass IgnoreVarianceClass { get; set; }

        public IEnumerable<IgnoreVarianceClass> IgnoreVarianceClasses { get; set; } 
    }
}
