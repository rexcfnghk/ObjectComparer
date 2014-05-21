using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Voyagers.Utilities.ObjectComparer.Tests.PartialClasses;
using Xunit;

namespace Voyagers.Utilities.ObjectComparer.Tests
{
    public class MetadataTypeTests
    {
        [Fact]
        public void IgnoreVarianceAttributeOnPropertyCanBeFoundSuccessfullyInMetadataClass()
        {
            // Arrange 
            PropertyInfo propertyInfo = typeof(Customer).GetProperty("Name");

            // Act
            bool result = ReflectionHelper.HasIgnoreVarianceAttribute(propertyInfo);

            // Asssert
            Assert.True(result);
        }

        [Fact]
        public void KeyAttributeCanBeFoundSuccessfullyInMetadataClass()
        {
            // Arrange 
            IEnumerable<PropertyInfo> propertyInfos;

            // Act
            bool result = ReflectionHelper.TryGetKeyAttriubte(typeof(Customer), out propertyInfos);
            List<PropertyInfo> propertyInfoList = propertyInfos.ToList();

            // Asssert
            Assert.True(result);
            Assert.NotEmpty(propertyInfoList);
            Assert.Equal(1, propertyInfoList.Count);
            Assert.Equal("Id", propertyInfoList[0].Name);
        }
    }
}
