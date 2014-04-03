using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Voyagers.Utilities.ObjectComparer.Tests.TestClasses;
using Xunit;

namespace Voyagers.Utilities.ObjectComparer.Tests
{
    public class KeyAttributeTests
    {
        private static IEnumerable<User> UserCollection1
        {
            get
            {
                return new List<User>
                {
                    new User { Id = 1, Name = "Foo" },
                    new User { Id = 2, Name = "Bar" },
                    new User { Id = 3, Name = "Baz" }
                }.AsReadOnly();
            }
        }

        private static IEnumerable<User> UserCollection2
        {
            get
            {
                return new List<User>
                {
                    new User { Id = 1, Name = "Foo" },
                    new User { Id = 3, Name = "Baz" },
                    new User { Id = 2, Name = "Bar" }
                }.AsReadOnly();
            }
        }

        public static IEnumerable<User> UserCollection3
        {
            get
            {
                return new List<User>
                {
                    new User { Id = 1, Name = "Foo" },
                    new User { Id = 2, Name = "Test" },
                    new User { Id = 4, Name = "Baz" }
                }.AsReadOnly();
            }
        }
        
        [Fact]
        public void UserContainingKeyAttributeCanBeDetected()
        {
            // Arrange
            IEnumerable<PropertyInfo> propertyInfos;

            // Act
            bool result = ReflectionHelper.TryGetKeyAttriubte(typeof(User), out propertyInfos);

            // Assert
            Assert.True(result);
            var infos = propertyInfos as IList<PropertyInfo> ?? propertyInfos.ToList();
            Assert.NotEmpty(infos);
            Assert.Equal(1, infos.Count());
            Assert.Equal("Id", infos[0].Name);
        }

        [Fact]
        public void ImmutableClassDoesNotContainKeyAttriubteCanBeDetected()
        {
            // Arrange
            IEnumerable<PropertyInfo> propertyInfos;

            // Act
            bool result = ReflectionHelper.TryGetKeyAttriubte(typeof(ImmutableClass), out propertyInfos);

            // Assert
            Assert.False(result);
            var infos = propertyInfos as IList<PropertyInfo> ?? propertyInfos.ToList();
            Assert.Empty(infos);
        }

        [Fact]
        public void ClassContainingTwoKeysCanBeDetected()
        {
            // Arrange
            IEnumerable<PropertyInfo> propertyInfos;

            // Act
            bool result = ReflectionHelper.TryGetKeyAttriubte(typeof(ClassWithTwoKeys), out propertyInfos);

            // Assert
            Assert.True(result);
            var infos = propertyInfos as IList<PropertyInfo> ?? propertyInfos.ToList();
            Assert.NotEmpty(infos);
            Assert.Equal(2, infos.Count());
            Assert.Equal("Id", infos[0].Name);
            Assert.Equal("Name", infos[1].Name);
        }

        [Fact]
        public void CollectionClassContainingDifferentCollectionsShouldCompareUsingKeys()
        {
            // Arrange
            var c1 = new CollectionClass
            {
                Users = UserCollection1
            };

            var c2 = new CollectionClass
            {
                Users = UserCollection2
            };

            // Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(c1, c2).ToList();

            // Assert
            Assert.Empty(variances);
        }

        [Fact]
        public void ListOfUsersShouldBeComparedUsingKeys()
        {
            // Arrange
            List<User> l1 = UserCollection1.ToList();
            List<User> l2 = UserCollection2.ToList();

            // Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(l1, l2).ToList();

            // Assert
            Assert.Empty(variances);
        }
    }
}
