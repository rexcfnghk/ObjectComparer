using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voyagers.Utilities.ObjectComparer.Tests.PartialClasses
{
    // Test class simulating a manually created partial class to included metadata
    [MetadataType(typeof(CustomerMetadata))]
    public partial class Customer
    {
    }
}
