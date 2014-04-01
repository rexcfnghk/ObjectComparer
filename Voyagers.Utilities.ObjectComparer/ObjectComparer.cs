using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Voyagers.Utilities.ObjectComparer
{
    public static class ObjectComparer
    {
        private static readonly IDictionary<Type, string> _aliases =
            new ReadOnlyDictionary<Type, string>(new Dictionary<Type, string>
            {
                { typeof(byte), "byte" },
                { typeof(sbyte), "sbyte" },
                { typeof(short), "short" },
                { typeof(ushort), "ushort" },
                { typeof(int), "int" },
                { typeof(uint), "uint" },
                { typeof(long), "long" },
                { typeof(ulong), "ulong" },
                { typeof(float), "float" },
                { typeof(double), "double" },
                { typeof(decimal), "decimal" },
                { typeof(object), "object" },
                { typeof(bool), "bool" },
                { typeof(char), "char" },
                { typeof(string), "string" },
                { typeof(void), "void" }
            });

        /// <summary>
        ///  Return object variances between two dynamic objects, serves as an entry point to all comparisons
        /// </summary>
        /// <param name="object1"></param>
        /// <param name="object2"></param>
        /// <returns>IEnumerable&lt;ObjectVariance&gt; that contains all differences</returns>
        public static IEnumerable<ObjectVariance> GetObjectVariances(dynamic object1, dynamic object2)
        {
            if (ReferenceEquals(object1, null) && ReferenceEquals(object2, null))
            {
                return Enumerable.Empty<ObjectVariance>();
            }

            // In case one of the objects is null
            IEnumerable<ObjectVariance> variances = CheckNullObjectsVariance(object1, object2);
            if (variances.Any())
            {
                return variances;
            }

            // ReSharper disable once PossibleNullReferenceException
            return object1.GetType() != object2.GetType()
                       ? new ObjectVariance("value", object1, object2, null).Yield()
                       : GetObjectVariances(object1, object2, null);
        }

        private static IEnumerable<ObjectVariance> GetObjectVariances(dynamic object1,
                                                                      dynamic object2,
                                                                      ObjectVariance parentVariance)
        {
            // Base case where object1 and object2 are PropertyInfo objects already
            string propertyName = null;
            var propertyInfo1 = object1 as PropertyInfo;
            var propertyInfo2 = object2 as PropertyInfo;
            if (propertyInfo1 != null && propertyInfo2 != null && propertyInfo1.Name == propertyInfo2.Name)
            {
                propertyName = propertyInfo1.Name;

                // ReSharper disable once PossibleNullReferenceException
                object1 = (object1 as PropertyInfo).GetValue(parentVariance.PropertyValue1);
                object2 = (object2 as PropertyInfo).GetValue(parentVariance.PropertyValue2);
            }

            // Both objects are null, required for inner property checking
            if (ReferenceEquals(object1, null) && ReferenceEquals(object2, null))
            {
                yield break;
            }

            // Null checks, subsituting objectVariance.PropertyName with propertyName if it is an inner level null check
            foreach (
                ObjectVariance objectVariance in CheckNullObjectsVariance(object1, object2))
            {
                // ReSharper disable once PossibleNullReferenceException
                yield return
                    new ObjectVariance(objectVariance.PropertyName,
                                       objectVariance.PropertyValue1,
                                       objectVariance.PropertyValue2,
                                       new ObjectVariance(propertyName,
                                                          parentVariance.PropertyValue1,
                                                          parentVariance.PropertyValue2,
                                                          null));
                yield break;
            }

            // ReSharper disable once PossibleNullReferenceException
            // Checked with ReferenceEquals(object1, null) already
            Type object1Type = object1.GetType();
            Type object2Type = object2.GetType();

            // Special case where the two objects are primitive type, if the two objects are not primitives,
            // there are two possibilities:
            // 1. object1 & object2 are properties to other types which can be further traversed
            // 2. object1 & object2 are top level objects
            if ((object1Type.IsPrimitive && object2Type.IsPrimitive) || (object1 is string && object2 is string))
            {
                if (object1.Equals(object2))
                {
                    yield break;
                }

                // parentVariance == null means primitives are being passed to compare
                // propertyName != null means deepest level reached within the object graph
                yield return
                    propertyName != null || parentVariance == null
                        ? new ObjectVariance(propertyName, object1, object2, parentVariance)
                        : parentVariance;
                yield break;
            }

            // parentVariance == null menas primitives are being passed passed to compare
            // propertyName != null means deepest level reached within the object graph
            if (propertyName != null || parentVariance == null)
            {
                parentVariance = new ObjectVariance(propertyName, object1, object2, parentVariance);
            }

            // For both objects implementing IEnumerable<T>
            Type object1GenericArgument, object2GenericArgument;
            if (TryGetIEnumerableGenericArgument(object1Type, out object1GenericArgument) &&
                TryGetIEnumerableGenericArgument(object2Type, out object2GenericArgument) &&
                object1GenericArgument == object2GenericArgument)
            {
                string genericAlias;
                _aliases.TryGetValue(object1GenericArgument, out genericAlias);
                IEnumerable<ObjectVariance> result = GetEnumerableVariances(object1,
                                                                            object2,
                                                                            parentVariance);
                foreach (ObjectVariance objectVariance in result)
                {

                    yield return objectVariance;
                }

                // Required for more than one difference in inner IEnumerables
                yield break;
            }

            foreach (
                ObjectVariance objectVariance in GetVariancesFromProperties(object1, object2, parentVariance))
            {
                yield return objectVariance;
            }
        }

        private static IEnumerable<ObjectVariance> GetEnumerableVariances(IEnumerable object1,
                                                                          IEnumerable object2,
                                                                          ObjectVariance parentVariance)
        {
            // Boxing here, but we cannot determine what generic type argument the caller will pass
            List<object> value1List = object1.Cast<object>().ToList();
            List<object> value2List = object2.Cast<object>().ToList();

            // If the count of IEnumerable is not equal, the two IEnumerable are definitely unequal
            if (value1List.Count != value2List.Count)
            {
                yield return
                    new ObjectVariance(
                        String.Format("{0}.Count()", parentVariance != null ? parentVariance.PropertyName : "this"),
                        value1List.Count,
                        value2List.Count,
                        parentVariance);

                // Yield break here as there is no reason to compare since their count is different
                yield break;
            }

            // Try to compare by position
            // TODO: Continue comparing even if Count is not equal / Use KeyAttribute in comparing
            for (int i = 0; i < value1List.Count; i++)
            {
                // As a test variance for each element in the IEnumerable
                var testParentVariance = new ObjectVariance(String.Format("this[{0}]", i),
                                                    value1List[i],
                                                    value2List[i],
                                                    parentVariance);
                foreach (ObjectVariance diff in GetObjectVariances(value1List[i], value2List[i], testParentVariance))
                {
                    yield return diff;
                }
            }
        }

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

            if (Attribute.GetCustomAttributes(type, typeof(KeyAttribute)).Count() > 1)
            {
                throw new ArgumentException("Type cannot contain more than one KeyAttribute", "type");
            }

            propertyInfos = from prop in GetPropertyInfos(type)
                            let attributes = Attribute.GetCustomAttributes(prop)
                            where attributes.OfType<KeyAttribute>().Any()
                            select prop;
            return propertyInfos.Any();
        }

        internal static IEnumerable<PropertyInfo> GetPropertyInfos(Type type)
        {
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

        private static IEnumerable<ObjectVariance> CheckNullObjectsVariance(object object1,
                                                                            object object2)
        {
            // One of the objects is null
            if (ReferenceEquals(object1, null))
            {
                yield return new ObjectVariance("value", null, object2, null);
                yield break;
            }

            if (ReferenceEquals(object2, null))
            {
                yield return new ObjectVariance("value", object1, null, null);
            }
        }

        private static IEnumerable<ObjectVariance> GetVariancesFromProperties(object object1,
                                                                              object object2,
                                                                              ObjectVariance parentVariance)
        {
            // ReferenceEquals here in case T is ValueType
            if (ReferenceEquals(object1, null))
            {
                throw new ArgumentNullException("object1");
            }

            if (ReferenceEquals(object2, null))
            {
                throw new ArgumentNullException("object2");
            }

            IEnumerable<PropertyInfo> object1PropertyInfos = GetPropertyInfos(object1.GetType());
            IEnumerable<PropertyInfo> object2PropertyInfos = GetPropertyInfos(object2.GetType());

            using (IEnumerator<PropertyInfo> propertyInfo1Enumerator = object1PropertyInfos.GetEnumerator(),
                                             propertyInfo2Enumerator = object2PropertyInfos.GetEnumerator())
            {
                while (propertyInfo1Enumerator.MoveNext() && propertyInfo2Enumerator.MoveNext())
                {
                    foreach (ObjectVariance diff in GetObjectVariances(propertyInfo1Enumerator.Current,
                                                                       propertyInfo2Enumerator.Current,
                                                                       parentVariance))
                    {
                        yield return diff;
                    }
                }
            }
        }
    }
}
