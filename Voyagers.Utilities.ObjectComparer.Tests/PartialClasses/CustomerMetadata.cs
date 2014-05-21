using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voyagers.Utilities.ObjectComparer.Attributes;

namespace Voyagers.Utilities.ObjectComparer.Tests.PartialClasses
{
    public class CustomerMetadata
    {
        [Key]
        public int Id { get; set; }

        [IgnoreVariance]
        public string Name { get; set; }
    }
}
