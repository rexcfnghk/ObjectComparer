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
