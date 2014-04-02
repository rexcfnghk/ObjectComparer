using System.Collections.Generic;
using System.Linq;
using Voyagers.Utilities.ObjectComparer.Tests.TestClasses;
using Xunit;

namespace Voyagers.Utilities.ObjectComparer.Tests
{
    public class IgnoreVarianceAttributeTests
    {
        [Fact]
        public void DifferenceInPropertiesWithIgnoreVarianceAreIgnored()
        {
            // Arrange
            var u1 = new User
            {
                Age = 10,
                Id = 1,
                Name = "Rex"
            };
            var u2 = new User
            {
                Age = 213,
                Id = 1,
                Name = "Rex"
            };

            // Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(u1, u2).ToList();

            // Assert
            Assert.Empty(variances);
        }

        [Fact]
        public void OtherVariancesInPropertiesShouldStillBeReported()
        {
            // Arrange
            var u1 = new User
            {
                Age = 10,
                Id = 1,
                Name = "Rex"
            };
            var u2 = new User
            {
                Age = 213,
                Id = 2,
                Name = "Rex"
            };

            // Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(u1, u2).ToList();

            // Assert
            Assert.NotEmpty(variances);
            Assert.Equal(1, variances.Count);

            Assert.Equal("Id", variances[0].PropertyName);
            Assert.Equal(1, variances[0].PropertyValue1);
            Assert.Equal(2, variances[0].PropertyValue2);

            Assert.NotNull(variances[0].ParentVariance);
            Assert.Null(variances[0].ParentVariance.PropertyName);
            Assert.Equal(u1, variances[0].ParentVariance.PropertyValue1);
            Assert.Equal(u2, variances[0].ParentVariance.PropertyValue2);

            Assert.Null(variances[0].ParentVariance.ParentVariance);
        }

        [Fact]
        public void DifferenceInClassWithIgnoreVarianceAreIgnored()
        {
            // Arrange
            var c1 = new IgnoreVarianceClass
            {
                Id = 201,
                IsHappy = false,
                Name = "sdafsdf"
            };

            var c2 = new IgnoreVarianceClass
            {
                Id = 1231,
                IsHappy = true,
                Name = "dafsdsdaf"
            };

            // Act
            List<ObjectVariance> variances = ObjectComparer.GetObjectVariances(c1, c2).ToList();

            // Assert
            Assert.Empty(variances);
        }
    }
}
