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
                            where Attribute.IsDefined(prop, typeof(KeyAttribute))
                            select prop;
            if (propertyInfos.Any())
            {
                return true;
            }

            // KeyAttribute is not declared on the type, search for MetadataTypeAttribute
            var metadataTypeAttributes =
                (MetadataTypeAttribute[])type.GetCustomAttributes(typeof(MetadataTypeAttribute));
            if (!metadataTypeAttributes.Any())
            {
                return false;
            }

            // MetadataAttribute exists on the type, find its metadata class and check if KeyAttribute exists
            // on any properties in the metadata class
            propertyInfos = from metadataTypeAttribute in metadataTypeAttributes
                            let metadataClassType = metadataTypeAttribute.MetadataClassType
                            from propertyInfo in GetPropertyInfos(metadataClassType)
                            let metadataClassPropertyInfo = metadataClassType.GetProperty(propertyInfo.Name)
                            where Attribute.IsDefined(metadataClassPropertyInfo, typeof(KeyAttribute))
                            select metadataClassPropertyInfo;

            return propertyInfos.Any();
        }

        internal static bool HasIgnoreVarianceAttribute(params PropertyInfo[] propertyInfos)
        {
            if (propertyInfos == null)
            {
                throw new ArgumentNullException("propertyInfos");
            }

            // Two scenarios here:
            // 1. IgnoreVarianceAttribute is applied on the property of the containing class
            // 2. IgnoreVarianceAttirubte is applied on the declaring type of the property
            if (propertyInfos.Any(pi => Attribute.IsDefined(pi, typeof(IgnoreVarianceAttribute)) ||
                                        HasIgnoreVarianceAttribute(pi.DeclaringType)))
            {
                return true;
            }

            // IgnoreVarianceAttribute is not declared on the type, search for MetadataTypeAttribute
            // Look for propertyInfos in the metadata class, if a metadata class exists
            var metadataPropertyInfos = (from propertyInfo in propertyInfos
                                         let metadataTypeAttributes =
                                             (MetadataTypeAttribute[])
                                             propertyInfo.DeclaringType.GetCustomAttributes(
                                                 typeof(MetadataTypeAttribute))
                                         where metadataTypeAttributes.Any()
                                         let metadataProperty =
                                             metadataTypeAttributes[0].MetadataClassType.GetProperty(propertyInfo.Name)
                                         where metadataProperty != null
                                         select metadataProperty).ToList();

            // No propertyInfos exist, because no metadata classes exist
            if (!metadataPropertyInfos.Any())
            {
                return false;
            }

            return (from metadataPropertyInfo in metadataPropertyInfos
                    where
                        Attribute.IsDefined(metadataPropertyInfo, typeof(IgnoreVarianceAttribute)) ||
                        HasIgnoreVarianceAttribute(metadataPropertyInfo.DeclaringType)
                    select metadataPropertyInfo).Any();
        }

        /// <summary>
        /// Check if any property of a given Type array contains IgnoreVarianceAttribute
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool HasIgnoreVarianceAttribute(params Type[] type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return type.Any(t => Attribute.IsDefined(t, typeof(IgnoreVarianceAttribute)));
        }

        internal static IEnumerable<PropertyInfo> GetPropertyInfos(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            // Skip indexers using p.GetIndexParameters().Length == 0
            return from prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                   where prop.CanRead && prop.GetIndexParameters().Length == 0
                   select prop;
        }

        /// <summary>
        /// Convert item of type <typeparam name="T">T</typeparam> to IEnumerable&lt;<typeparam name="T">T</typeparam>&gt;
        /// </summary>
        /// <typeparam name="T">Generic argument for IEnumerable</typeparam>
        /// <param name="item">Item that is being yielded</param>
        /// <returns>An IEnumerable of T</returns>
        internal static IEnumerable<T> Yield<T>(this T item)
        {
            if (ReferenceEquals(item, null))
            {
                throw new ArgumentNullException("item");
            }

            yield return item;
        }

        /// <summary>
        /// Returns true if Type of obj is string, DateTime, or obj.GetType().IsPrimitive returns true
        /// </summary>
        /// <param name="objs">Objs params array</param>
        /// <returns>True or False</returns>
        internal static bool ShouldIgnoreVariance(params object[] objs)
        {
            return objs != null &&
                   objs.All(o => !ReferenceEquals(o, null) && (o is string || o is DateTime || o.GetType().IsPrimitive));
        }
    }
}
