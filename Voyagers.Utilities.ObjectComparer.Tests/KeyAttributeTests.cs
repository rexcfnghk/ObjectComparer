using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Voyagers.Utilities.ObjectComparer.Tests.TestClasses;
using Xunit;

namespace Voyagers.Utilities.ObjectComparer.Tests
{
    public class KeyAttributeTests
    {
        [Fact]
        public void UserContainingKeyAttributeCanBeDetected()
        {
            // Arrange
            IEnumerable<PropertyInfo> propertyInfos;

            // Act
            ObjectComparer.TryGetKeyAttriubte(typeof(User), out propertyInfos);

            // Assert
            var infos = propertyInfos as IList<PropertyInfo> ?? propertyInfos.ToList();
            Assert.NotEmpty(infos);
            Assert.Equal(1, infos.Count());
            Assert.Equal("Id", infos[0].Name);
        }

        [Fact]
        public void ClassContainingTwoKeysCanBeDetected()
        {
            // Arrange
            IEnumerable<PropertyInfo> propertyInfos;

            // Act
            ObjectComparer.TryGetKeyAttriubte(typeof(ClassWithTwoKeys), out propertyInfos);

            // Assert
            var infos = propertyInfos as IList<PropertyInfo> ?? propertyInfos.ToList();
            Assert.NotEmpty(infos);
            Assert.Equal(2, infos.Count());
            Assert.Equal("Id", infos[0].Name);
            Assert.Equal("Name", infos[1].Name);
        }
    }
}
