using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
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

            Assert.Equal("Id", variances[0].PropertyName);
            Assert.Equal(1, variances[0].PropertyValue1);
            Assert.Equal(2, variances[0].PropertyValue2);
            Assert.NotNull(variances[0].ParentVariance);

            Assert.Null(variances[0].ParentVariance.PropertyName);
            Assert.Equal(user1, variances[0].ParentVariance.PropertyValue1);
            Assert.Equal(user2, variances[0].ParentVariance.PropertyValue2);
            Assert.Null(variances[0].ParentVariance.ParentVariance);

            Assert.Equal("Name", variances[1].PropertyName);
            Assert.Equal("Foo", variances[1].PropertyValue1);
            Assert.Equal("Bar", variances[1].PropertyValue2);
            Assert.NotNull(variances[1].ParentVariance);

            Assert.Null(variances[1].ParentVariance.PropertyName);
            Assert.Equal(user1, variances[1].ParentVariance.PropertyValue1);
            Assert.Equal(user2, variances[1].ParentVariance.PropertyValue2);
            Assert.Null(variances[1].ParentVariance.ParentVariance);

            Assert.Equal("Id", variances[2].PropertyName);
            Assert.Equal(1, variances[2].PropertyValue1);
            Assert.Equal(2, variances[2].PropertyValue2);
            Assert.NotNull(variances[2].ParentVariance);

            Assert.Equal("Role", variances[2].ParentVariance.PropertyName);
            Assert.Equal(role1, variances[2].ParentVariance.PropertyValue1);
            Assert.Equal(role2, variances[2].ParentVariance.PropertyValue2);
            Assert.NotNull(variances[2].ParentVariance.ParentVariance);

            Assert.Null(variances[2].ParentVariance.ParentVariance.PropertyName);
            Assert.Equal(user1, variances[2].ParentVariance.ParentVariance.PropertyValue1);
            Assert.Equal(user2, variances[2].ParentVariance.ParentVariance.PropertyValue2);
            Assert.Null(variances[2].ParentVariance.ParentVariance.ParentVariance);

            Assert.Equal("Name", variances[3].PropertyName);
            Assert.Equal("Administrator", variances[3].PropertyValue1);
            Assert.Equal("Tester", variances[3].PropertyValue2);
            Assert.NotNull(variances[3].ParentVariance);

            Assert.Equal("Role", variances[3].ParentVariance.PropertyName);
            Assert.Equal(role1, variances[3].ParentVariance.PropertyValue1);
            Assert.Equal(role2, variances[3].ParentVariance.PropertyValue2);
            Assert.NotNull(variances[3].ParentVariance.ParentVariance);

            Assert.Null(variances[3].ParentVariance.ParentVariance.PropertyName);
            Assert.Equal(user1, variances[3].ParentVariance.ParentVariance.PropertyValue1);
            Assert.Equal(user2, variances[3].ParentVariance.ParentVariance.PropertyValue2);
            Assert.Null(variances[3].ParentVariance.ParentVariance.ParentVariance);
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

            Assert.Equal("Id", variances[0].PropertyName);
            Assert.Equal(1, variances[0].PropertyValue1);
            Assert.Equal(2, variances[0].PropertyValue2);
            Assert.NotNull(variances[0].ParentVariance);

            Assert.Null(variances[0].ParentVariance.PropertyName);
            Assert.Equal(user1, variances[0].ParentVariance.PropertyValue1);
            Assert.Equal(user2, variances[0].ParentVariance.PropertyValue2);
            Assert.Null(variances[0].ParentVariance.ParentVariance);

            Assert.Equal("Extra object in Roles with key {Id=2}", variances[1].PropertyName);
            Assert.Equal(role2, variances[1].PropertyValue1);
            Assert.Null(variances[1].PropertyValue2);
            Assert.NotNull(variances[1].ParentVariance);

            Assert.Equal("Roles", variances[1].ParentVariance.PropertyName);
            Assert.Equal(user1.Roles, variances[1].ParentVariance.PropertyValue1);
            Assert.Equal(user2.Roles, variances[1].ParentVariance.PropertyValue2);
            Assert.NotNull(variances[1].ParentVariance.ParentVariance);

            Assert.Null(variances[1].ParentVariance.ParentVariance.PropertyName);
            Assert.Equal(user1, variances[1].ParentVariance.ParentVariance.PropertyValue1);
            Assert.Equal(user2, variances[1].ParentVariance.ParentVariance.PropertyValue2);
            Assert.Null(variances[1].ParentVariance.ParentVariance.ParentVariance);
        }
    }
}
