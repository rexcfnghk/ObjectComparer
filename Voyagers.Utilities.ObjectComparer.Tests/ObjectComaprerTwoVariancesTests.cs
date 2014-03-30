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
            Assert.Equal("IEnumerable<char> String1 at index 1", variances[1].PropertyName);
            Assert.Equal('a', variances[1].PropertyValue1);
            Assert.Equal('e', variances[1].PropertyValue2);

            // Second's parent
            Assert.NotNull(variances[1].ParentVariance);
            Assert.Equal("String1", variances[1].ParentVariance.PropertyName);
            Assert.Equal("Tast", variances[1].ParentVariance.PropertyValue1);
            Assert.Equal("Test", variances[1].ParentVariance.PropertyValue2);

            // Second's parent's parent
            Assert.NotNull(variances[1].ParentVariance.ParentVariance);
            Assert.Null(variances[1].ParentVariance.ParentVariance.PropertyName);
            Assert.Equal(c1, variances[1].ParentVariance.ParentVariance.PropertyValue1);
            Assert.Equal(c2, variances[1].ParentVariance.ParentVariance.PropertyValue2);
            Assert.Null(variances[1].ParentVariance.ParentVariance.ParentVariance);
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
        public void CollectionClassWithTwoDifferencesShouldReturnTwoVariances()
        {
            // Arrange
            var c1 = new CollectionClass
            {
                ImmutableClasses = new List<ImmutableClass>
                {
                    new ImmutableClass(1, "tast"),
                }
            };

            var c2 = new CollectionClass
            {
                ImmutableClasses = new List<ImmutableClass>
                {
                    new ImmutableClass(2, "test"),
                }
            };

            // Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(c1, c2).ToList();

            // Assert
            Assert.NotEmpty(variances);
            Assert.Equal(2, variances.Count); // List comparison is count-based, so should be 2 differences

            // First variance
            Assert.Equal("Int1", variances[0].PropertyName);
            Assert.Equal(c1.ImmutableClasses, variances[0].ParentVariance.PropertyValue1);
            Assert.Equal(c2.ImmutableClasses, variances[0].ParentVariance.PropertyValue2);
            Assert.Equal(1, variances[0].PropertyValue1);
            Assert.Equal(2, variances[0].PropertyValue2);

            // Second variance
            Assert.Equal("String1 at index 1", variances[1].PropertyName);
            Assert.Equal(c1.ImmutableClasses, variances[1].ParentVariance.PropertyValue1);
            Assert.Equal(c2.ImmutableClasses, variances[1].ParentVariance.PropertyValue2);
            Assert.Equal('a', variances[1].PropertyValue1);
            Assert.Equal('e', variances[1].PropertyValue2);
        }
    }
}
