using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voyagers.Utilities.ObjectComparer.Tests.TestClasses
{
    internal class RoleWithUserCollection
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public ICollection<UserWithRoleCollection> Users { get; set; } 
    }
}
