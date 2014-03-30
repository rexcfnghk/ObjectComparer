using System.Collections.Generic;
using System.Linq;
using Voyagers.Utilities.ObjectComparer.Tests.TestClasses;
using Xunit;

namespace Voyagers.Utilities.ObjectComparer.Tests
{
    public class ObjectComparerTests
    {
        [Fact]
        public void TwoEqualIntsShouldReturnNoVariance()
        {
            // Arrange and Act
            IEnumerable<ObjectVariance> variances = ObjectComparer.GetObjectVariances(1, 1);

            // Assert
            Assert.Empty(variances);
        }

        [Fact]
        public void TwoDifferentIntsShouldReturnVariance()
        {
            // Arrange and Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(1, 2).ToList();

            // Assert
            Assert.NotEmpty(variances);
            Assert.Equal(1, variances.Count);
            Assert.Null(variances[0].PropertyName);
            Assert.Equal(1, variances[0].PropertyValue1);
            Assert.Equal(2, variances[0].PropertyValue2);
            Assert.Equal(null, variances[0].ParentVariance);
        }

        [Fact]
        public void TwoDifferentTypesShouldReturnVariance()
        {
            // Arrange and Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(1, "test").ToList();

            // Assert
            Assert.NotEmpty(variances);
            Assert.Equal(1, variances.Count);
            Assert.Equal("value", variances[0].PropertyName);
            Assert.Equal(1, variances[0].PropertyValue1);
            Assert.Equal("test", variances[0].PropertyValue2);
            Assert.Equal(null, variances[0].ParentVariance);
        }

        [Fact]
        public void TwoNullsShouldReturnNoVariance()
        {
            // Arrange and Act
            IEnumerable<ObjectVariance> variances = ObjectComparer.GetObjectVariances(null, null);

            // Assert
            Assert.Empty(variances);
        }

        [Fact]
        public void Object2NullReferenceShouldReturnVariance()
        {
            // Arrange and Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(1, null).ToList();

            // Assert
            Assert.NotEmpty(variances);
            Assert.Equal(1, variances.Count);
            Assert.Equal("value", variances[0].PropertyName);
            Assert.Equal(1, variances[0].PropertyValue1);
            Assert.Equal(null, variances[0].PropertyValue2);
            Assert.Equal(null, variances[0].ParentVariance);
        }

        [Fact]
        public void Object1NullReferenceShouldReturnVariance()
        {
            // Arrange and Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(null, 1).ToList();

            // Assert
            Assert.NotEmpty(variances);
            Assert.Equal(1, variances.Count);
            Assert.Equal("value", variances[0].PropertyName);
            Assert.Equal(null, variances[0].PropertyValue1);
            Assert.Equal(1, variances[0].PropertyValue2);
            Assert.Equal(null, variances[0].ParentVariance);
        }

        [Fact]
        public void EqualOneLevelStringsShouldReturnNoVariance()
        {
            // Arrange and Act
            IEnumerable<ObjectVariance> variances = ObjectComparer.GetObjectVariances("test", "test");

            // Assert
            Assert.Empty(variances);
        }

        [Fact]
        public void UnequalStringsShouldReturnVariance()
        {
            // Arrange and Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances("test", "tast").ToList();

            // Assert
            Assert.NotEmpty(variances);
            Assert.NotNull(variances[0]);
            Assert.Equal("test", variances[0].PropertyValue1);
            Assert.Equal("tast", variances[0].PropertyValue2);
            Assert.Null(variances[0].PropertyName);
            Assert.Null(variances[0].ParentVariance);
        }

        [Fact]
        public void EqualListOfIntsShouldReturnNoVariance()
        {
            // Arrange
            var list1 = new List<int> { 1, 2, 3 };
            var list2 = new List<int> { 1, 2, 3 };

            // Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(list1, list2).ToList();

            // Assert
            Assert.Empty(variances);
        }

        [Fact]
        public void UnequalListOfIntsShouldReturnVariance()
        {
            // Arrange
            var list1 = new List<int> { 1, 2, 3 };
            var list2 = new List<int> { 1, 2, 2 };

            // Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(list1, list2).ToList();

            // Assert
            Assert.NotEmpty(variances);
            Assert.Equal(1, variances.Count);
            Assert.Equal("IEnumerable<int> at index 2", variances[0].PropertyName);
            Assert.Equal(3, variances[0].PropertyValue1);
            Assert.Equal(2, variances[0].PropertyValue2);

            // Parent
            Assert.NotNull(variances[0].ParentVariance);
            Assert.Null(variances[0].ParentVariance.PropertyName);
            Assert.Equal(list1, variances[0].ParentVariance.PropertyValue1);
            Assert.Equal(list2, variances[0].ParentVariance.PropertyValue2);
        }

        [Fact]
        public void DifferentReferencesOfObjectsShouldReturnNoVariance()
        {
            // Arrange
            var obj1 = new object();
            var obj2 = new object();

            // Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(obj1, obj2).ToList();

            // Assert
            Assert.Empty(variances);
        }

        [Fact]
        public void ClassWithSameContentsShouldReturnNoVariance()
        {
            // Assert
            var c1 = new MutableClass
            {
                Int1 = 1,
                String1 = "Test"
            };

            var c2 = new MutableClass
            {
                Int1 = 1,
                String1 = "Test"
            };

            // Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(c1, c2).ToList();

            // Assert
            Assert.Empty(variances);
        }

        [Fact]
        public void ClassWithDifferentIntsShouldReturnVariance()
        {
            // Assert
            var c1 = new MutableClass
            {
                Int1 = 1,
                String1 = "Test"
            };

            var c2 = new MutableClass
            {
                Int1 = 2,
                String1 = "Test"
            };

            // Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(c1, c2).ToList();

            // Assert
            Assert.NotEmpty(variances);
            Assert.Equal(1, variances.Count);
            Assert.Equal("Int1", variances[0].PropertyName);
            Assert.Equal(c1, variances[0].ParentVariance.PropertyValue1);
            Assert.Equal(c2, variances[0].ParentVariance.PropertyValue2);
            Assert.Equal(1, variances[0].PropertyValue1);
            Assert.Equal(2, variances[0].PropertyValue2);
        }

        [Fact]
        public void ClassWithDifferentStringsShouldReturnVariance()
        {
            // Assert
            var c1 = new MutableClass
            {
                Int1 = 1,
                String1 = "Tast"
            };

            var c2 = new MutableClass
            {
                Int1 = 1,
                String1 = "Test"
            };

            // Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(c1, c2).ToList();

            // Assert
            Assert.NotEmpty(variances);
            Assert.Equal(1, variances.Count);

            // Parent
            Assert.Equal("Tast", variances[0].PropertyValue1);
            Assert.Equal("Test", variances[0].PropertyValue2);
            Assert.Equal("String1", variances[0].PropertyName);
            Assert.NotNull(variances[0].ParentVariance);

            Assert.Equal(c1, variances[0].ParentVariance.PropertyValue1);
            Assert.Equal(c2, variances[0].ParentVariance.PropertyValue2);
            Assert.Null(variances[0].ParentVariance.PropertyName);
            Assert.Null(variances[0].ParentVariance.ParentVariance);
        }

        [Fact]
        public void ClassWithNullStringsShouldReturnNoVariance()
        {
            // Assert
            var c1 = new MutableClass
            {
                Int1 = 1,
                String1 = null
            };

            var c2 = new MutableClass
            {
                Int1 = 1,
                String1 = null
            };

            // Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(c1, c2).ToList();

            // Assert
            Assert.Empty(variances);
        }

        [Fact]
        public void ClassWithOneNullStringShouldReturnVariance()
        {
            // Assert
            var c1 = new MutableClass
            {
                Int1 = 1,
                String1 = null
            };

            var c2 = new MutableClass
            {
                Int1 = 1,
                String1 = "test"
            };

            // Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(c1, c2).ToList();

            // Assert
            Assert.NotEmpty(variances);
            Assert.Equal(1, variances.Count);
            Assert.Equal("value", variances[0].PropertyName);
            Assert.Equal(null, variances[0].PropertyValue1);
            Assert.Equal("test", variances[0].PropertyValue2);

            // Parent
            Assert.NotNull(variances[0].ParentVariance);
            Assert.Equal("String1", variances[0].ParentVariance.PropertyName);
            Assert.Equal(c1, variances[0].ParentVariance.PropertyValue1);
            Assert.Equal(c2, variances[0].ParentVariance.PropertyValue2);
            Assert.Null(variances[0].ParentVariance.ParentVariance);
        }

        [Fact]
        public void ClassWithSameListContentsShouldReturnNoVariance()
        {
            // Arrange
            var c1 = new CollectionClass
            {
                Ints = new List<int> { 1, 2, 3 }
            };

            var c2 = new CollectionClass
            {
                Ints = new List<int> { 1, 2, 3 }
            };

            // Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(c1, c2).ToList();

            // Assert
            Assert.Empty(variances);
        }

        [Fact]
        public void ClassWithOneEmptyListShouldReturnVariance()
        {
            // Arrange
            var list1 = new List<int> { 1, 2, 3 };
            var list2 = Enumerable.Empty<int>();
            var c1 = new CollectionClass
            {
                Ints = list1
            };

            var c2 = new CollectionClass
            {
                Ints = list2
            };

            // Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(c1, c2).ToList();

            // Assert
            Assert.NotEmpty(variances);
            Assert.Equal(1, variances.Count);
            Assert.Equal("IEnumerable<int> Ints.Count()", variances[0].PropertyName);
            Assert.Equal(3, variances[0].PropertyValue1);
            Assert.Equal(0, variances[0].PropertyValue2);

            // Parent
            Assert.NotNull(variances[0].ParentVariance);
            Assert.Equal(list1, variances[0].ParentVariance.PropertyValue1);
            Assert.Equal(list2, variances[0].ParentVariance.PropertyValue2);
            Assert.Equal("Ints", variances[0].ParentVariance.PropertyName);

            // Parent's parent
            Assert.NotNull(variances[0].ParentVariance.ParentVariance);
            Assert.Null(variances[0].ParentVariance.ParentVariance.PropertyName);
            Assert.Equal(c1, variances[0].ParentVariance.ParentVariance.PropertyValue1);
            Assert.Equal(c2, variances[0].ParentVariance.ParentVariance.PropertyValue2);
            Assert.Null(variances[0].ParentVariance.ParentVariance.ParentVariance);
        }

        [Fact]
        public void ClassWithSameInnerClassContentsShouldReturnNoVariance()
        {
            // Arrange
            var c1 = new MutableClass
            {
                Int1 = 1,
                String1 = "test",
                Immutable1 = new ImmutableClass(2, "test2")
            };

            var c2 = new MutableClass
            {
                Int1 = 1,
                String1 = "test",
                Immutable1 = new ImmutableClass(2, "test2")
            };

            // Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(c1, c2).ToList();

            // Arrange
            Assert.Empty(variances);
        }

        [Fact]
        public void ClassWithOneNullInnerClassContentsShouldReturnVariance()
        {
            // Arrange
            var inner = new ImmutableClass(2, "test");
            var c1 = new MutableClass
            {
                Int1 = 1,
                String1 = "test",
                Immutable1 = inner
            };

            var c2 = new MutableClass
            {
                Int1 = 1,
                String1 = "test",
                Immutable1 = null
            };

            // Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(c1, c2).ToList();

            // Assert
            Assert.NotEmpty(variances);
            Assert.Equal(1, variances.Count);
            Assert.Equal("value", variances[0].PropertyName);
            Assert.Equal(inner, variances[0].PropertyValue1);
            Assert.Equal(null, variances[0].PropertyValue2);

            // Parent
            Assert.NotNull(variances[0].ParentVariance);
            Assert.Equal("Immutable1", variances[0].ParentVariance.PropertyName);
            Assert.Equal(c1, variances[0].ParentVariance.PropertyValue1);
            Assert.Equal(c2, variances[0].ParentVariance.PropertyValue2);
            Assert.Null(variances[0].ParentVariance.ParentVariance);
        }

        [Fact]
        public void ClassWithDifferentInnerClassContentsShouldReturnVariance()
        {
            // Arrange
            var inner1 = new ImmutableClass(2, "test");
            var inner2 = new ImmutableClass(2, "tast");

            var c1 = new MutableClass
            {
                Int1 = 1,
                String1 = "test",
                Immutable1 = inner1
            };

            var c2 = new MutableClass
            {
                Int1 = 1,
                String1 = "test",
                Immutable1 = inner2
            };

            // Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(c1, c2).ToList();

            // Assert
            Assert.NotEmpty(variances);
            Assert.Equal(1, variances.Count);

            // Parent
            Assert.Equal("String1", variances[0].PropertyName);
            Assert.Equal("test", variances[0].PropertyValue1);
            Assert.Equal("tast", variances[0].PropertyValue2);
            
            // Parent's Parent
            Assert.NotNull(variances[0].ParentVariance);
            Assert.Equal("Immutable1", variances[0].ParentVariance.PropertyName);
            Assert.Equal(inner1, variances[0].ParentVariance.PropertyValue1);
            Assert.Equal(inner2, variances[0].ParentVariance.PropertyValue2);

            // Parent's Parent's Parent
            Assert.NotNull(variances[0].ParentVariance.ParentVariance);
            Assert.Equal(c1, variances[0].ParentVariance.ParentVariance.PropertyValue1);
            Assert.Equal(c2, variances[0].ParentVariance.ParentVariance.PropertyValue2);
            Assert.Null(variances[0].ParentVariance.ParentVariance.PropertyName);
            Assert.Null(variances[0].ParentVariance.ParentVariance.ParentVariance);
        }
    }
}
