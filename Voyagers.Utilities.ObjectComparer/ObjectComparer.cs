using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;

namespace Voyagers.Utilities.ObjectComparer
{
    public static class ObjectComparer
    {
        private static readonly HashSet<object> TraversedObjects = new HashSet<object>();
        private static readonly object TraversedObjectLock = new object();

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
            IEnumerable<ObjectVariance> variances = CheckNullObjectsVariance(object1, object2, null);
            if (variances.Any())
            {
                return variances;
            }

            // ReSharper disable once PossibleNullReferenceException
            Type object1Type = object1.GetType();
            Type object2Type = object2.GetType();

            // When one of the types have IgnoreVarianceAttribute on it
            if (ReflectionHelper.HasIgnoreVarianceAttribute(object1Type, object2Type))
            {
                return Enumerable.Empty<ObjectVariance>();
            }

            // Empty TraversedObjects
            TraversedObjects.Clear();

            // ReSharper disable once PossibleNullReferenceException
            return object1.GetType() != object2.GetType()
                       ? new ObjectVariance("Type", object1, object2, null).Yield()
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
                // Found IgnoreVarianceAttribute
                if (ReflectionHelper.HasIgnoreVarianceAttribute(propertyInfo1, propertyInfo2))
                {
                    yield break;
                }

                propertyName = propertyInfo1.Name;

                // ReSharper disable once PossibleNullReferenceException
                object1 = (object1 as PropertyInfo).GetValue(parentVariance.PropertyValue1);
                object2 = (object2 as PropertyInfo).GetValue(parentVariance.PropertyValue2);

                // Check if already traversed
                if (!ReflectionHelper.ShouldIgnoreVariance(object1, object2) && AreObjectsTraversed(object1, object2))
                {
                    yield break;
                }
            }

            // Both objects are null, required for inner property checking
            if (ReferenceEquals(object1, null) && ReferenceEquals(object2, null))
            {
                yield break;
            }

            // Null checks, two scenarios
            // 1. It is a propertyInfo that can be further traversed
            // 2. It is a top level object
            object nullCheck1 = propertyInfo1 ?? object1;
            object nullCheck2 = propertyInfo2 ?? object2;
            foreach (
                ObjectVariance objectVariance in CheckNullObjectsVariance(nullCheck1, nullCheck2, parentVariance))
            {
                // ReSharper disable once PossibleNullReferenceException
                yield return objectVariance;
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
            if (ReflectionHelper.ShouldIgnoreVariance(object1, object2))
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
            if (ReflectionHelper.TryGetIEnumerableGenericArgument(object1Type, out object1GenericArgument) &&
                ReflectionHelper.TryGetIEnumerableGenericArgument(object2Type, out object2GenericArgument) &&
                object1GenericArgument == object2GenericArgument)
            {
                // Found IgnoreVarianceAttribute
                if (ReflectionHelper.HasIgnoreVarianceAttribute(object1GenericArgument))
                {
                    yield break;
                }

                IEnumerable<PropertyInfo> propertyInfos;
                IEnumerable<ObjectVariance> result = ReflectionHelper.TryGetKeyAttriubte(object1GenericArgument,
                                                                                         out propertyInfos)
                                                         ? GetEnumerableVariancesByKey(object1,
                                                                                       object2,
                                                                                       propertyInfos,
                                                                                       parentVariance)
                                                         : GetEnumerableVariancesByPosition(object1,
                                                                                            object2,
                                                                                            parentVariance);
                foreach (ObjectVariance objectVariance in result)
                {
                    yield return objectVariance;
                }

                // Required for more than one difference in inner IEnumerables
                yield break;
            }

            // Add to traversed HashSet
            lock (TraversedObjectLock)
            {
                TraversedObjects.Add(object1);
                TraversedObjects.Add(object2);
            }

            // Compare by property
            foreach (
                ObjectVariance objectVariance in 
                GetVariancesFromProperties(object1, object2, parentVariance))
            {
                yield return objectVariance;
            }
        }

        private static IEnumerable<ObjectVariance> GetEnumerableVariancesByKey(IEnumerable enumerable1,
                                                                               IEnumerable enumerable2,
                                                                               IEnumerable<PropertyInfo> propertyInfos,
                                                                               ObjectVariance parentVariance)
        {
            List<string> propertyInfosList = propertyInfos.Select(p => p.Name).ToList();
            string propertyNames = String.Join(", ", propertyInfosList);

            // Try to group by properties having the KeyAttribute applied using Dynamic LINQ
            // IGroupedEnumerable with Key => Anonymous object with the key properties, 
            //                         Count => Count of grouped objects
            //                         Value => The grouped object themselves
            //                         Compared => Boolean flag for determining whether we can skip comparisons or not
            IQueryable query1 =
                enumerable1.AsQueryable()
                           .GroupBy(String.Format("new ({0})", propertyNames), "it")
                           .Select("new (it.Key, it.Count() as Count, it as Value, false as Compared)");
            IQueryable query2 =
                enumerable2.AsQueryable()
                           .GroupBy(String.Format("new ({0})", propertyNames), "it")
                           .Select("new (it.Key, it.Count() as Count, it as Value, false as Compared)");

            // Make sure if query1 contains more items than query2, or vice versa, can be detected
            foreach (
                ObjectVariance objectVariance in
                    KeyPropertiesComparer.GetSetDifferenceVariances(query1, query2, parentVariance))
            {
                yield return objectVariance;
            }

            foreach (dynamic group1 in query1)
            {
                foreach (dynamic group2 in query2)
                {
                    if (group1.Compared || group2.Compared)
                    {
                        continue;
                    }

                    if (!Object.Equals(group1.Key, group2.Key))
                    {
                        continue;
                    }

                    if (group1.Count > 1 || group2.Count > 1)
                    {
                        throw new InvalidOperationException("The IEnumerable contains objects with the same key");
                    }

                    // Here we already guaranteed there is one value per group for each key
                    foreach (
                        ObjectVariance objectVariance in
                            GetEnumerableVariancesByPosition(group1.Value, group2.Value, parentVariance, group1.Key))
                    {
                        yield return objectVariance;
                    }

                    group1.Compared = true;
                    group2.Compared = true;
                }
            }
        }

        private static IEnumerable<ObjectVariance> GetEnumerableVariancesByPosition(IEnumerable object1,
                                                                                    IEnumerable object2,
                                                                                    ObjectVariance parentVariance,
                                                                                    object key = null)
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
            for (int i = 0; i < value1List.Count; i++)
            {
                // Optional key object used in here since GetEnumerableVariancesByKey will eventually route to here
                // propertyName will be assigned "this[i]" when comparing by position or key.ToString() when comparing by key
                string propertyName = key == null ? String.Format("this[{0}]", i) : key.ToString();

                if (AreObjectsTraversed(value1List[i], value2List[i]))
                {
                    yield break;
                }

                // As a test variance for each element in the IEnumerable
                var testParentVariance = new ObjectVariance(propertyName,
                                                            value1List[i],
                                                            value2List[i],
                                                            parentVariance);
                foreach (
                    ObjectVariance objectVariance in
                        GetObjectVariances(value1List[i], value2List[i], testParentVariance))
                {
                    yield return objectVariance;
                }
            }
        }
      
        private static IEnumerable<ObjectVariance> CheckNullObjectsVariance(object object1,
                                                                            object object2,
                                                                            ObjectVariance parentVariance)
        {
            var propertyInfo1 = object1 as PropertyInfo;
            var propertyInfo2 = object2 as PropertyInfo;

            // One of the objects is null
            if (propertyInfo1 != null && ReferenceEquals(propertyInfo1.GetValue(parentVariance.PropertyValue1), null))
            {
                // object2 is a PropertyInfo
                if (propertyInfo2 != null)
                {
                    object property2Value = propertyInfo2.GetValue(parentVariance.PropertyValue2);
                    yield return new ObjectVariance(propertyInfo2.Name, null, property2Value, parentVariance);
                    yield break;
                }

                // object2 is a primitive or string, or DateTime
                yield return new ObjectVariance(null, null, object2, parentVariance);
                yield break;
            }

            if (propertyInfo2 != null && ReferenceEquals(propertyInfo2.GetValue(parentVariance.PropertyValue2), null))
            {
                // object1 is a PropertyInfo
                if (propertyInfo1 != null)
                {
                    object property1Value = propertyInfo1.GetValue(parentVariance.PropertyValue1);
                    yield return new ObjectVariance(propertyInfo1.Name, property1Value, null, parentVariance);
                    yield break;
                }

                // object1 is a primitive or string, or DateTime
                yield return new ObjectVariance(null, null, object2, parentVariance);
            }
        }

        private static IEnumerable<ObjectVariance> GetVariancesFromProperties(object object1,
                                                                              object object2,
                                                                              ObjectVariance parentVariance)
        {
            // ReferenceEquals instead of == because == may be overridden
            if (ReferenceEquals(object1, null))
            {
                throw new ArgumentNullException("object1");
            }

            // ReferenceEquals instead of == because == may be overridden
            if (ReferenceEquals(object2, null))
            {
                throw new ArgumentNullException("object2");
            }

            IEnumerable<PropertyInfo> object1PropertyInfos = ReflectionHelper.GetPropertyInfos(object1.GetType());
            IEnumerable<PropertyInfo> object2PropertyInfos = ReflectionHelper.GetPropertyInfos(object2.GetType());

            using (IEnumerator<PropertyInfo> propertyInfo1Enumerator = object1PropertyInfos.GetEnumerator(),
                                             propertyInfo2Enumerator = object2PropertyInfos.GetEnumerator())
            {
                while (propertyInfo1Enumerator.MoveNext() && propertyInfo2Enumerator.MoveNext())
                {
                    // Found IgnoreVarianceAttribute
                    if (ReflectionHelper.HasIgnoreVarianceAttribute(propertyInfo1Enumerator.Current,
                                                                    propertyInfo2Enumerator.Current))
                    {
                        continue;
                    }

                    foreach (ObjectVariance objectVariance in GetObjectVariances(propertyInfo1Enumerator.Current,
                                                                                 propertyInfo2Enumerator.Current,
                                                                                 parentVariance))
                    {
                        yield return objectVariance;
                    }
                }
            }
        }

        private static bool AreObjectsTraversed(params object[] objs)
        {
            lock (TraversedObjectLock)
            {
                return objs.All(o => TraversedObjects.Contains(o));
            }
        }
    }
}
