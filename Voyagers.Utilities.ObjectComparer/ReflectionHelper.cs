using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Voyagers.Utilities.ObjectComparer.Attributes;

namespace Voyagers.Utilities.ObjectComparer
{
    internal static class ReflectionHelper
    {
        /// <summary>
        /// Returns if <param name="type">type</param> 
        /// implements IEnumerable&lt;T&gt;, and <param name="genericArgument"></param> containing 
        /// the generic argument of IEnumerable&lt;T&gt;
        /// </summary>
        /// <param name="type"></param>
        /// <param name="genericArgument">out parameter containing the type argument of IEnumerable&lt;T&gt;</param>
        /// <returns></returns>
        internal static bool TryGetIEnumerableGenericArgument(Type type, out Type genericArgument)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            // Check whether type implements IEnumerable<T> first
            Type ienumerableType = type.GetInterfaces()
                                       .FirstOrDefault(
                                           interfaceType =>
                                           interfaceType.IsGenericType &&
                                           interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            genericArgument = ienumerableType != null ? ienumerableType.GetGenericArguments()[0] : null;
            return genericArgument != null;
        }

        /// <summary>
        /// Returns true if any members of<param name="type">type</param> contain the KeyAttribute
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyInfos">out parameter of PropertyInfos containing all properties with KeyAttribute</param>
        /// <returns></returns>
        internal static bool TryGetKeyAttriubte(Type type, out IEnumerable<PropertyInfo> propertyInfos)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            propertyInfos = from prop in GetPropertyInfos(type)
                            let attributes = Attribute.GetCustomAttributes(prop)
                            where attributes.OfType<KeyAttribute>().Any()
                            select prop;
            return propertyInfos.Any();
        }

        internal static bool HasIgnoreVarianceAttribute(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw new ArgumentNullException("propertyInfo");
            }

            return Attribute.GetCustomAttribute(propertyInfo, typeof(IgnoreVarianceAttribute)) != null;
        }

        internal static bool HasIgnoreVarianceAttribute(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return Attribute.GetCustomAttribute(type, typeof(IgnoreVarianceAttribute)) != null;
        }

        internal static IEnumerable<PropertyInfo> GetPropertyInfos(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            // Skip indexers using p.GetIndexParameters().Length == 0
            return
                type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);
        }

        /// <summary>
        /// Convert item of type <typeparam name="T">T</typeparam> to IEnumerable&lt;<typeparam name="T">T</typeparam>&gt;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        internal static IEnumerable<T> Yield<T>(this T item)
        {
            if (ReferenceEquals(item, null))
            {
                throw new ArgumentNullException("item");
            }

            yield return item;
        }
    }
}
