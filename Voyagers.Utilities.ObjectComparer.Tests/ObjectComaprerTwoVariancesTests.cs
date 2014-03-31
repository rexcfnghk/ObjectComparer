using System;
using System.Collections.Generic;
using System.Linq;
using Voyagers.Utilities.ObjectComparer.Tests.TestClasses;
using Xunit;

namespace Voyagers.Utilities.ObjectComparer.Tests
{
    public class ObjectComparerTwoVariancesTest
    {
        [Fact]
        public void ClassWithTwoDifferencesShouldReturnTwoVariances()
        {
            // Arrange
            var c1 = new MutableClass
            {
                Int1 = 1,
                String1 = "Tast"
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
            Assert.Equal(2, variances.Count);

            // First variance
            Assert.Equal("Int1", variances[0].PropertyName);
            Assert.Equal(1, variances[0].PropertyValue1);
            Assert.Equal(2, variances[0].PropertyValue2);
            
            // First's parent
            Assert.NotNull(variances[0].ParentVariance);
            Assert.Null(variances[0].ParentVariance.PropertyName);
            Assert.Equal(c1, variances[0].ParentVariance.PropertyValue1);
            Assert.Equal(c2, variances[0].ParentVariance.PropertyValue2);

            // Second variance
            Assert.Equal("String1", variances[1].PropertyName);
            Assert.Equal("Tast", variances[1].PropertyValue1);
            Assert.Equal("Test", variances[1].PropertyValue2);

            // Second's parent
            Assert.NotNull(variances[1].ParentVariance);
            Assert.Null(variances[1].ParentVariance.PropertyName);
            Assert.Equal(c1, variances[1].ParentVariance.PropertyValue1);
            Assert.Equal(c2, variances[1].ParentVariance.PropertyValue2);
            Assert.Null(variances[1].ParentVariance.ParentVariance);
        }

        [Fact]
        public void PrimitivesHolderWithTwoDifferenceShouldReturnTwoVariances()
        {
            // Arrange
            var c1 = new PrimitivesHolder
            {
                Int1 = 1,
                Char1 = 'e'
            };

            var c2 = new PrimitivesHolder
            {
                Int1 = 2,
                Char1 = 'a'
            };

            // Arrange
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(c1, c2).ToList();

            // Assert
            Assert.NotEmpty(variances);
            Assert.Equal(2, variances.Count);

            // First variance
            Assert.Equal(1, variances[0].PropertyValue1);
            Assert.Equal(2, variances[0].PropertyValue2);
            Assert.Equal("Int1", variances[0].PropertyName);

            // Parent
            Assert.NotNull(variances[0].ParentVariance);
            Assert.Equal(c1, variances[0].ParentVariance.PropertyValue1);
            Assert.Equal(c2, variances[0].ParentVariance.PropertyValue2);
            Assert.Null(variances[0].ParentVariance.PropertyName);
            Assert.Null(variances[0].ParentVariance.ParentVariance);

            // Second variance
            Assert.Equal('e', variances[1].PropertyValue1);
            Assert.Equal('a', variances[1].PropertyValue2);
            Assert.Equal("Char1", variances[1].PropertyName);

            // Parent
            Assert.NotNull(variances[1].ParentVariance);
            Assert.Equal(c1, variances[1].ParentVariance.PropertyValue1);
            Assert.Equal(c2, variances[1].ParentVariance.PropertyValue2);
            Assert.Null(variances[1].ParentVariance.PropertyName);
            Assert.Null(variances[1].ParentVariance.ParentVariance);
        }

        [Fact]
        public void PrimitivesHolderWithTwoDifferentStringsShouldReturnVariances()
        {
            // Arrange
            var l1 = new List<string> { "tast", "test" };
            var l2 = new List<string> { "t1st", "test" };
            var c1 = new PrimitivesHolder
            {
                Strings = l1
            };

            var c2 = new PrimitivesHolder
            {
                Strings = l2
            };

            // Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(c1, c2).ToList();

            // Assert
            Assert.NotEmpty(variances);
            Assert.Equal(1, variances.Count);

            // First variance
            Assert.Equal("tast", variances[0].PropertyValue1);
            Assert.Equal("t1st", variances[0].PropertyValue2);
            Assert.Equal("IEnumerable<string> Strings at index 0", variances[0].PropertyName);

            // First variance's parent's parent
            Assert.NotNull(variances[0].ParentVariance);
            Assert.Equal(l1, variances[0].ParentVariance.PropertyValue1);
            Assert.Equal(l2, variances[0].ParentVariance.PropertyValue2);
            Assert.Equal("Strings", variances[0].ParentVariance.PropertyName);

            // First variance's parent's parent
            Assert.NotNull(variances[0].ParentVariance.ParentVariance);
            Assert.Equal(c1, variances[0].ParentVariance.ParentVariance.PropertyValue1);
            Assert.Equal(c2, variances[0].ParentVariance.ParentVariance.PropertyValue2);
            Assert.Null(variances[0].ParentVariance.ParentVariance.PropertyName);
            Assert.Null(variances[0].ParentVariance.ParentVariance.ParentVariance);
        }

        [Fact]
        public void CollectionClassWithTwoDifferencesShouldReturnTwoVariances()
        {
            // Arrange
            var inner1 = new ImmutableClass(1, "tast");
            var inner2 = new ImmutableClass(2, "test");
            var c1 = new CollectionClass
            {
                ImmutableClasses = new List<ImmutableClass>
                {
                    inner1
                }
            };

            var c2 = new CollectionClass
            {
                ImmutableClasses = new List<ImmutableClass>
                {
                    inner2
                }
            };

            // Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(c1, c2).ToList();

            // Assert
            Assert.NotEmpty(variances);
            Assert.Equal(2, variances.Count); // List comparison is count-based, so should be 2 differences

            // First variance
            Assert.Equal("Int1", variances[0].PropertyName);
            Assert.Equal(1, variances[0].PropertyValue1);
            Assert.Equal(2, variances[0].PropertyValue2);

            // First variance's parent
            Assert.NotNull(variances[0].ParentVariance);
            Assert.Equal("IEnumerable<ImmutableClass> ImmutableClasses at index 0", variances[0].ParentVariance.PropertyName);
            Assert.Equal(inner1, variances[0].ParentVariance.PropertyValue1);
            Assert.Equal(inner2, variances[0].ParentVariance.PropertyValue2);

            // First variance's parent's parent
            Assert.NotNull(variances[0].ParentVariance.ParentVariance);
            Assert.Null(variances[0].ParentVariance.ParentVariance.PropertyName);
            Assert.Equal(c1, variances[0].ParentVariance.ParentVariance.PropertyValue1);
            Assert.Equal(c2, variances[0].ParentVariance.ParentVariance.PropertyValue2);

            // Second variance
            Assert.Equal("String1", variances[1].PropertyName);
            Assert.Equal("tast", variances[1].PropertyValue1);
            Assert.Equal("test", variances[1].PropertyValue2);

            // Second variance's parent
            Assert.NotNull(variances[1].ParentVariance);
            Assert.Equal("IEnumerable<ImmutableClass> ImmutableClasses at index 0", variances[1].ParentVariance.PropertyName);
            Assert.Equal(inner1, variances[1].ParentVariance.PropertyValue1);
            Assert.Equal(inner2, variances[1].ParentVariance.PropertyValue2);

            // Second variance's parent's parent
            Assert.NotNull(variances[1].ParentVariance.ParentVariance);
            Assert.Null(variances[1].ParentVariance.ParentVariance.PropertyName);
            Assert.Equal(c1, variances[1].ParentVariance.ParentVariance.PropertyValue1);
            Assert.Equal(c2, variances[1].ParentVariance.ParentVariance.PropertyValue2);
        }
    }
}
