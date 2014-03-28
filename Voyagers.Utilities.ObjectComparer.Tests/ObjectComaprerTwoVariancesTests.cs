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
            Assert.Equal(c1, variances[0].ParentVariance.Value1);
            Assert.Equal(c2, variances[0].ParentVariance.Value2);
            Assert.Equal(2, variances[0].Level);
            Assert.Equal(1, variances[0].Value1);
            Assert.Equal(2, variances[0].Value2);

            // Second variance
            Assert.Equal("String1 at index 1", variances[1].PropertyName);
            Assert.Equal(c1, variances[1].ParentVariance.Value1);
            Assert.Equal(c2, variances[1].ParentVariance.Value2);
            Assert.Equal(4, variances[1].Level);
            Assert.Equal('a', variances[1].Value1);
            Assert.Equal('e', variances[1].Value2);
        }

        [Fact]
        public void CollectionClassWithTwoDifferencesShouldReturnTwoVariances()
        {
            // Arrange
            var c1 = new CollectionClass
            {
                ImmutableClasses = new List<ImmutableClass>
                {
                    new ImmutableClass(1, "test"),
                    new ImmutableClass(2, "tast")
                }
            };

            var c2 = new CollectionClass
            {
                ImmutableClasses = new List<ImmutableClass>
                {
                    new ImmutableClass(2, "test"),
                    new ImmutableClass(2, "test")
                }
            };

            // Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(c1, c2).ToList();

            // Assert
            Assert.NotEmpty(variances);
            Assert.Equal(2, variances.Count); // List comparison is count-based, so should be 2 differences

            // First variance
            Assert.Equal("Int1", variances[0].PropertyName);
            Assert.Equal(c1.ImmutableClasses, variances[0].ParentVariance.Value1);
            Assert.Equal(c2.ImmutableClasses, variances[0].ParentVariance.Value2);
            Assert.Equal(5, variances[0].Level);
            Assert.Equal(1, variances[0].Value1);
            Assert.Equal(2, variances[0].Value2);

            // Second variance
            Assert.Equal("String1 at index 1", variances[1].PropertyName);
            Assert.Equal(c1.ImmutableClasses, variances[1].ParentVariance.Value1);
            Assert.Equal(c2.ImmutableClasses, variances[1].ParentVariance.Value2);
            Assert.Equal(5, variances[1].Level);
            Assert.Equal('a', variances[1].Value1);
            Assert.Equal('e', variances[1].Value2);
        }
    }
}
