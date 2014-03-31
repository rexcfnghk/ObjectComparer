using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Voyagers.Utilities.ObjectComparer.Tests.TestClasses
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
