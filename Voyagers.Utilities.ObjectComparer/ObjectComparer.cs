using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Voyagers.Utilities.ObjectComparer
{
    public static class ObjectComparer
    {
        private static readonly Dictionary<Type, string> _aliases = new Dictionary<Type, string>
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
        };

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
            if (object1Type.IsPrimitive && object2Type.IsPrimitive /* && object1 is string && object2 is string */)
            {
                if (object1.Equals(object2))
                {
                    yield break;
                }

                yield return new ObjectVariance(propertyName ?? "value", object1, object2, parentVariance);
                yield break;
            }

            // For both objects implementing IEnumerable<T>
            Type object1GenericArgument, object2GenericArgument;
            if (TryGetIEnumerableGenericArgument(object1Type, out object1GenericArgument) &&
                TryGetIEnumerableGenericArgument(object2Type, out object2GenericArgument) &&
                object1GenericArgument == object2GenericArgument)
            {
                string genericAlias;
                _aliases.TryGetValue(object1GenericArgument, out genericAlias);
                parentVariance = new ObjectVariance(propertyName, object1, object2, parentVariance);
                IEnumerable<ObjectVariance> result = GetEnumerableVariances(object1,
                                                                            object2,
                                                                            parentVariance);
                foreach (ObjectVariance objectVariance in result)
                {
                    _aliases.TryGetValue(object1GenericArgument, out genericAlias);
                    yield return
                        new ObjectVariance(
                            String.Format("IEnumerable<{0}> " + objectVariance.PropertyName, genericAlias, propertyName)
                                  .TrimExtraSpacesBetweenWords(),
                            objectVariance.PropertyValue1,
                            objectVariance.PropertyValue2,
                            objectVariance.ParentVariance);
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
                yield return new ObjectVariance("{1}.Count()", value1List.Count, value2List.Count, parentVariance);

                // Yield break here as there is no reason to compare since their count is different
                yield break;
            }

            // Try to compare by position
            // TODO: Continue comparing even if Count is not equal / Use KeyAttribute in comparing
            for (int i = 0; i < value1List.Count; i++)
            {
                foreach (
                    ObjectVariance diff in
                        GetObjectVariances(value1List[i], value2List[i], parentVariance))
                {
                    yield return
                        new ObjectVariance("{1} at index " + i,
                                           diff.PropertyValue1,
                                           diff.PropertyValue2,
                                           parentVariance);
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

            // Skip indexers using p.GetIndexParameters().Length == 0
            IEnumerable<PropertyInfo> object1PropertyInfos =
                object1.GetType()
                       .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                       .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                       .ToList();
            IEnumerable<PropertyInfo> object2PropertyInfos =
                object2.GetType()
                       .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                       .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                       .ToList();

            using (IEnumerator<PropertyInfo> propertyInfo1Enumerator = object1PropertyInfos.GetEnumerator(),
                                             propertyInfo2Enumerator = object2PropertyInfos.GetEnumerator())
            {
                while (propertyInfo1Enumerator.MoveNext() && propertyInfo2Enumerator.MoveNext())
                {
                    // Tried return GetObjectVariances(propertyInfo1Enumerator.Current, propertyInfo2Enumerator.Current,
                    // ++level, object1, object2). Does not work
                    var testParentVariance = new ObjectVariance(parentVariance != null ? parentVariance.PropertyName : null,
                                                                object1,
                                                                object2,
                                                                parentVariance);
                    foreach (ObjectVariance diff in GetObjectVariances(propertyInfo1Enumerator.Current,
                                                                       propertyInfo2Enumerator.Current,
                                                                       testParentVariance))
                    {
                        yield return diff;
                    }
                }
            }
        }
    }
}
