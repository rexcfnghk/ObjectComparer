using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Voyagers.Utilities.ObjectComparer
{
    public static class ObjectComparer
    {
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
            IEnumerable<ObjectVariance> variances = CheckNullObjectsVariance(object1, object2, 1, null, null);
            if (variances.Any())
            {
                return variances;
            }

            // ReSharper disable once PossibleNullReferenceException
            if (object1.GetType() != object2.GetType())
            {
                return new List<ObjectVariance> { new ObjectVariance("value", object1, object2, 1, null, null) }
                    .AsReadOnly();
            }

            return GetObjectVariances(object1, object2, 1, null, null);
        }

        private static IEnumerable<ObjectVariance> GetObjectVariances(dynamic object1,
                                                                      dynamic object2,
                                                                      int level,
                                                                      object parent1,
                                                                      object parent2)
        {
            // Base case where object1 and object2 are PropertyInfo objects already
            string propertyName = null;
            var propertyInfo1 = object1 as PropertyInfo;
            var propertyInfo2 = object2 as PropertyInfo;
            if (propertyInfo1 != null && propertyInfo2 != null && propertyInfo1.Name == propertyInfo2.Name)
            {
                propertyName = propertyInfo1.Name;
                object1 = (object1 as PropertyInfo).GetValue(parent1);
                object2 = (object2 as PropertyInfo).GetValue(parent2);
            }

            // Both objects are null, required for inner property checking
            if (ReferenceEquals(object1, null) && ReferenceEquals(object2, null))
            {
                yield break;
            }

            // Null checks, subsituting objectVariance.PropertyName with propertyName if it is an inner level null check
            foreach (
                ObjectVariance objectVariance in CheckNullObjectsVariance(object1, object2, level, parent1, parent2))
            {
                yield return
                    new ObjectVariance(propertyName ?? objectVariance.PropertyName,
                                       objectVariance.Value1,
                                       objectVariance.Value2,
                                       objectVariance.Level,
                                       objectVariance.ParentReference1,
                                       objectVariance.ParentReference2);
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
                if (!object1.Equals(object2))
                {
                    yield return new ObjectVariance(propertyName ?? "value", object1, object2, level, parent1, parent2);
                }
            }

            // For both objects implementing IEnumerable<T>
            Type object1GenericArgument, object2GenericArgument;
            if (TryGetIEnumerableGenericArgument(object1Type, out object1GenericArgument) &&
                TryGetIEnumerableGenericArgument(object2Type, out object2GenericArgument) &&
                object1GenericArgument == object2GenericArgument)
            {
                var result = GetEnumerableVariances(object1, object2, level, parent1, parent2);
                foreach (ObjectVariance objectVariance in result)
                {
                    yield return
                        new ObjectVariance(String.Format(objectVariance.PropertyName, propertyName ?? "IEnumerable"),
                                           objectVariance.Value1,
                                           objectVariance.Value2,
                                           objectVariance.Level,
                                           objectVariance.ParentReference1,
                                           objectVariance.ParentReference2);
                }

                // Required for more than one difference in inner IEnumerables
                yield break;
            }

            foreach (
                ObjectVariance objectVariance in GetVariancesFromProperties(object1, object2, level))
            {
                yield return objectVariance;
            }
        }

        private static IEnumerable<ObjectVariance> GetEnumerableVariances(IEnumerable object1,
                                                                          IEnumerable object2,
                                                                          int level,
                                                                          object parent1,
                                                                          object parent2)
        {
            // Boxing here, but we cannot determine what generic type argument the caller will pass
            List<object> value1List = object1.Cast<object>().ToList();
            List<object> value2List = object2.Cast<object>().ToList();

            // If the count of IEnumerable is not equal, the two IEnumerable are definitely unequal
            if (value1List.Count != value2List.Count)
            {
                yield return new ObjectVariance("{0}.Count()", object1, object2, ++level, parent1, parent2);
            }

            // Try to compare by position
            // TODO: Continue comparing even if Count is not equal / Use KeyAttribute in comparing
            else
            {
                for (int i = 0; i < value1List.Count; i++)
                {
                    foreach (
                        ObjectVariance diff in
                            GetObjectVariances(value1List[i], value2List[i], level, parent1, parent2))
                    {
                        yield return
                            new ObjectVariance("{0} at index " + i,
                                               diff.Value1,
                                               diff.Value2,
                                               diff.Level + 1,
                                               diff.ParentReference1,
                                               diff.ParentReference2);
                    }
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

        internal static bool IsIEnumerable(Type type)
        {
            Type genericArgument;
            return TryGetIEnumerableGenericArgument(type, out genericArgument);
        }

        private static IEnumerable<ObjectVariance> CheckNullObjectsVariance(object object1,
                                                                            object object2,
                                                                            int level,
                                                                            object parent1,
                                                                            object parent2)
        {
            // One of the objects is null
            if (ReferenceEquals(object1, null))
            {
                yield return new ObjectVariance("value", null, object2, level, parent1, parent2);
                yield break;
            }

            if (ReferenceEquals(object2, null))
            {
                yield return new ObjectVariance("value", object1, null, level, parent1, parent2);
            }
        }

        private static IEnumerable<ObjectVariance> GetVariancesFromProperties(object object1,
                                                                              object object2,
                                                                              int level)
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
                    foreach (ObjectVariance diff in GetObjectVariances(propertyInfo1Enumerator.Current,
                                                                       propertyInfo2Enumerator.Current,
                                                                       ++level,
                                                                       object1,
                                                                       object2))
                    {
                        yield return diff;
                    }
                }
            }
        }
    }
}
