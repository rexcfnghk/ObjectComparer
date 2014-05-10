using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voyagers.Utilities.ObjectComparer.Tests.TestClasses;
using Xunit;

namespace Voyagers.Utilities.ObjectComparer.Tests
{
    public sealed class CircularReferenceTests
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

        [Fact]
        public void UserWithRoleCollectionDoesNotCauseStackOverflow()
        {
            // Arrange
            var role1 = new RoleWithUserCollection
            {
                Id = 1,
                Name = "Administrator"
            };

            var role2 = new RoleWithUserCollection
            {
                Id = 2,
                Name = "Tester"
            };

            var user1 = new UserWithRoleCollection
            {
                Id = 1,
                Name = "Foo",
                Age = 10
            };

            var user2 = new UserWithRoleCollection
            {
                Id = 2,
                Name = "Foo",
                Age = 10,
            };

            role1.Users = new HashSet<UserWithRoleCollection> { user1, user2 };
            role2.Users = new HashSet<UserWithRoleCollection> { user1 };
            user1.Roles = new HashSet<RoleWithUserCollection> { role1, role2 };
            user2.Roles = new HashSet<RoleWithUserCollection> { role1 };

            // Act and Assert
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(user1, user2).ToList();

            // Assert
            Assert.NotEmpty(variances);
        }
    }
}
