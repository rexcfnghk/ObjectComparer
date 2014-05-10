using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Voyagers.Utilities.ObjectComparer.Tests.TestClasses;
using Xunit;

namespace Voyagers.Utilities.ObjectComparer.Tests
{
    public sealed class KeyAttributeTests
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

        private static IEnumerable<User> UserCollection3
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

        private static IEnumerable<User> UserCollection4
        {
            get
            {
                return new List<User>
                {
                    new User { Id = 1, Name = "Foo" },
                    new User { Id = 2, Name = "Bar" },
                    new User { Id = 3, Name = "Baz" },
                    new User { Id = 4, Name = "Test"}
                }.AsReadOnly();
            }
        }

        private static IEnumerable<ClassWithTwoKeys> TwoKeyCollection1
        {
            get
            {
                return new List<ClassWithTwoKeys>
                {
                    new ClassWithTwoKeys { Id = 1, Name = "Abc" },
                    new ClassWithTwoKeys { Id = 2, Name = "Cba" }
                };
            }
        }

        private static IEnumerable<ClassWithTwoKeys> TwoKeyCollection2
        {
            get
            {
                return new List<ClassWithTwoKeys>
                {
                    new ClassWithTwoKeys { Id = 1, Name = "Cba" },
                    new ClassWithTwoKeys { Id = 2, Name = "Cba" }
                };
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

        [Fact]
        public void ListOfUsersWithDifferentKeysShouldYieldVariance()
        {
            // Arrange
            List<User> l1 = UserCollection1.ToList();
            List<User> l2 = UserCollection3.ToList();

            // Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(l1, l2).ToList();

            // Assert
            Assert.NotEmpty(variances);
            Assert.Equal(3, variances.Count);
            Assert.Equal("Extra object in IEnumerable 1 with key {Id=3}", variances[0].PropertyName);
            Assert.Equal(l1[2], variances[0].PropertyValue1);
            Assert.Null(variances[0].PropertyValue2);

            Assert.NotNull(variances[0].ParentVariance);
            Assert.Equal(l1, variances[0].ParentVariance.PropertyValue1);
            Assert.Equal(l2, variances[0].ParentVariance.PropertyValue2);
            Assert.Null(variances[0].ParentVariance.PropertyName);

            Assert.Null(variances[0].ParentVariance.ParentVariance);

            Assert.Equal("Extra object in IEnumerable 2 with key {Id=4}", variances[1].PropertyName);
            Assert.Null(variances[1].PropertyValue1);
            Assert.Equal(l2[2], variances[1].PropertyValue2);

            Assert.NotNull(variances[1].ParentVariance);
            Assert.Equal(l1, variances[1].ParentVariance.PropertyValue1);
            Assert.Equal(l2, variances[1].ParentVariance.PropertyValue2);
            Assert.Null(variances[1].ParentVariance.PropertyName);

            Assert.Null(variances[1].ParentVariance.ParentVariance);


            Assert.Equal("Bar", variances[2].PropertyValue1);
            Assert.Equal("Test", variances[2].PropertyValue2);
            Assert.Equal("Name", variances[2].PropertyName);

            Assert.NotNull(variances[2].ParentVariance);
            Assert.Equal(l1[1], variances[2].ParentVariance.PropertyValue1);
            Assert.Equal(l2[1], variances[2].ParentVariance.PropertyValue2);
            Assert.Equal("{Id=2}", variances[2].ParentVariance.PropertyName);

            Assert.NotNull(variances[2].ParentVariance.ParentVariance);
            Assert.Equal(l1, variances[2].ParentVariance.ParentVariance.PropertyValue1);
            Assert.Equal(l2, variances[2].ParentVariance.ParentVariance.PropertyValue2);
            Assert.Null(variances[2].ParentVariance.ParentVariance.PropertyName);
        }

        [Fact]
        public void DifferentLengthsOfUserListsShouldAlsoReturnVariances()
        {
            // Arrange
            List<User> l1 = UserCollection1.ToList();
            List<User> l2 = UserCollection4.ToList();

            // Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(l1, l2).ToList();

            // Assert
            Assert.NotEmpty(variances);
            Assert.Equal(1, variances.Count);

            Assert.Equal("Extra object in IEnumerable 2 with key {Id=4}", variances[0].PropertyName);
            Assert.Null(variances[0].PropertyValue1);
            Assert.Equal(l2[3], variances[0].PropertyValue2);

            Assert.NotNull(variances[0].ParentVariance);
            Assert.Equal(l1, variances[0].ParentVariance.PropertyValue1);
            Assert.Equal(l2, variances[0].ParentVariance.PropertyValue2);
            Assert.Null(variances[0].ParentVariance.PropertyName);

            Assert.Null(variances[0].ParentVariance.ParentVariance);
        }

        [Fact]
        public void DifferentLengthsOfUserListsReversedShouldAlsoReturnVariances()
        {
            // Arrange
            List<User> l1 = UserCollection4.ToList();
            List<User> l2 = UserCollection1.ToList();

            // Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(l1, l2).ToList();

            // Assert
            Assert.NotEmpty(variances);
            Assert.Equal(1, variances.Count);

            Assert.Equal("Extra object in IEnumerable 1 with key {Id=4}", variances[0].PropertyName);
            Assert.Equal(l1[3], variances[0].PropertyValue1);
            Assert.Null(variances[0].PropertyValue2);

            Assert.NotNull(variances[0].ParentVariance);
            Assert.Equal(l1, variances[0].ParentVariance.PropertyValue1);
            Assert.Equal(l2, variances[0].ParentVariance.PropertyValue2);
            Assert.Null(variances[0].ParentVariance.PropertyName);

            Assert.Null(variances[0].ParentVariance.ParentVariance);
        }

        [Fact]
        public void ClassesWithTwoKeysCanBeDetected()
        {
            // Arrange
            List<ClassWithTwoKeys> l1 = TwoKeyCollection1.ToList();
            List<ClassWithTwoKeys> l2 = TwoKeyCollection2.ToList();

            // Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(l1, l2).ToList();

            // Assert
            Assert.NotEmpty(variances);
            Assert.Equal(2, variances.Count);

            Assert.Equal("Extra object in IEnumerable 1 with key {Id=1, Name=Abc}", variances[0].PropertyName);
            Assert.Equal(l1[0], variances[0].PropertyValue1);
            Assert.Null(variances[0].PropertyValue2);

            Assert.NotNull(variances[0].ParentVariance);
            Assert.Equal(l1, variances[0].ParentVariance.PropertyValue1);
            Assert.Equal(l2, variances[0].ParentVariance.PropertyValue2);
            Assert.Null(variances[0].ParentVariance.PropertyName);

            Assert.Null(variances[0].ParentVariance.ParentVariance);

            Assert.Equal("Extra object in IEnumerable 2 with key {Id=1, Name=Cba}", variances[1].PropertyName);
            Assert.Null(variances[1].PropertyValue1);
            Assert.Equal(l2[0], variances[1].PropertyValue2);

            Assert.NotNull(variances[1].ParentVariance);
            Assert.Equal(l1, variances[1].ParentVariance.PropertyValue1);
            Assert.Equal(l2, variances[1].ParentVariance.PropertyValue2);
            Assert.Null(variances[1].ParentVariance.PropertyName);

            Assert.Null(variances[1].ParentVariance.ParentVariance);
        }
    }
}
