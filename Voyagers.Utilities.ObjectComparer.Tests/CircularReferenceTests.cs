using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voyagers.Utilities.ObjectComparer.Tests.TestClasses;
using Xunit;

namespace Voyagers.Utilities.ObjectComparer.Tests
{
    public class CircularReferenceTests
    {
        [Fact]
        public void CircularReferenceDoesNotCauseStackOverflow()
        {
            // Arrange
            var role1 = new Role
            {
                Id = 1,
                Name = "Administrator"
            };

            var user1 = new User
            {
                Id = 1,
                Name = "Foo"
            };

            var role2 = new Role
            {
                Id = 2,
                Name = "Tester"
            };

            var user2 = new User
            {
                Id = 2,
                Name = "Bar"
            };

            role1.User = user1;
            user1.Role = role1;
            role2.User = user2;
            user2.Role = role2;

            // Act and Assert
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(user1, user2).ToList();

            // Assert
            Assert.NotEmpty(variances);
        }
    }
}
