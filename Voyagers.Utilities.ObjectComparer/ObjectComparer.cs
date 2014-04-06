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
            Type object1Type = object1.GetType();
            Type object2Type = object2.GetType();

            // When one of the types have IgnoreVarianceAttribute on it
            if (ReflectionHelper.HasIgnoreVarianceAttribute(object1Type) ||
                ReflectionHelper.HasIgnoreVarianceAttribute(object2Type))
            {
                return Enumerable.Empty<ObjectVariance>();
            }

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
                if (ReflectionHelper.HasIgnoreVarianceAttribute(propertyInfo1) ||
                    ReflectionHelper.HasIgnoreVarianceAttribute(propertyInfo2))
                {
                    yield break;
                }

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

            // Boxing here, but we cannot determine what generic type argument the caller will pass
            var query1 = enumerable1.AsQueryable().GroupBy(String.Format("new ({0})", propertyNames), "it").Select("new (it.Key, it.Count() as Count, it as Value, false as Compared)");
            var query2 = enumerable2.AsQueryable().GroupBy(String.Format("new ({0})", propertyNames), "it").Select("new (it.Key, it.Count() as Count, it as Value, false as Compared)");

            foreach (
                ObjectVariance objectVariance in
                    KeyPropertiesComparer.GetSetDifferenceVariances(query1, query2, parentVariance))
            {
                yield return objectVariance;
            }

            //IEnumerator enumerator1 = query1.GetEnumerator();
            //IEnumerator enumerator2 = query2.GetEnumerator();

            //try
            //{
            //    while (enumerator1.MoveNext() && enumerator2.MoveNext())
            //    {
            //        dynamic group1 = enumerator1.Current;
            //        dynamic group2 = enumerator2.Current;

            //        if (group1.Compared || group2.Compared)
            //        {
            //            continue;
            //        }

            //        if (!Object.Equals(group1.Key, group2.Key))
            //        {
            //            continue;
            //        }

            //        if (group1.Count > 1 || group2.Count > 1)
            //        {
            //            throw new InvalidOperationException("The IEnumerable contains objects with the same key");
            //        }

            //        IEnumerator object1Enumerator = group1.Value.GetEnumerator();
            //        IEnumerator object2Enumerator = group2.Value.GetEnumeartor();

            //        try
            //        {
            //            while (object1Enumerator.MoveNext() && object2Enumerator.MoveNext())
            //            {
            //                var testVariance = new ObjectVariance(group1.Key.ToString(),
            //                                                      object1Enumerator.Current,
            //                                                      object2Enumerator.Current,
            //                                                      parentVariance);

            //                foreach (
            //                    ObjectVariance variance in
            //                        GetObjectVariances(object1Enumerator.Current,
            //                                           object2Enumerator.Current,
            //                                           testVariance))
            //                {
            //                    yield return variance;
            //                }

            //                group1.Compared = true;
            //                group2.Compared = true;
            //            }
            //        }
            //        finally
            //        {
            //            if (object1Enumerator as IDisposable != null)
            //            {
            //                ((IDisposable)object1Enumerator).Dispose();
            //            }

            //            if (object2Enumerator as IDisposable != null)
            //            {
            //                ((IDisposable)object2Enumerator).Dispose();
            //            }
            //        }
            //    }
            //}
            //finally
            //{
            //    if (enumerator1 as IDisposable != null)
            //    {
            //        ((IDisposable)enumerator1).Dispose();
            //    }

            //    if (enumerator2 as IDisposable != null)
            //    {
            //        ((IDisposable)enumerator2).Dispose();
            //    }
            //}

            foreach (dynamic group1 in query1)
            {
                foreach (dynamic group2 in query2)
                {
                    //var key1 = group1.Key;
                    //var key2 = group2.Key;

                    // Must access the Value property here to use foreach loop below
                    //var group1List = ;
                    //var group2List = ;
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

                    // Bug here where I cannot make sure exactly one element exists in each grouping
                    foreach (var object1 in group1.Value)
                    {
                        foreach (var object2 in group2.Value)
                        {
                            var testVariance = new ObjectVariance(group1.Key.ToString(),
                                                                  object1,
                                                                  object2,
                                                                  parentVariance);
                            foreach (ObjectVariance objectVariance in GetObjectVariances(object1, object2, testVariance))
                            {
                                yield return objectVariance;
                            }
                        }
                    }

                    group1.Compared = true;
                    group2.Compared = true;
                }
            }



            //throw new NotImplementedException();
            // TODO: Get the keyObject from object1, find the corresponding object2 in list2, do comparison if found, yield variance if not found
            //foreach (object object1 in list1)
            //{
            //    foreach (object object2 in list2)
            //    {
            //        if (propertyInfoList.All(p => p.GetValue(object1).Equals(p.GetValue(object2))))
            //        {
            //            foreach (ObjectVariance objectVariance in GetObjectVariances(object1, object2, parentVariance))
            //            {
            //                yield return objectVariance;
            //            }
            //        }
            //        else
            //        {
            //            
            //        }
            //    }
            //}
        }

        private static IEnumerable<ObjectVariance> GetEnumerableVariancesByPosition(IEnumerable object1,
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
                foreach (
                    ObjectVariance objectVariance in
                        GetObjectVariances(value1List[i], value2List[i], testParentVariance))
                {
                    yield return objectVariance;
                }
            }
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

            IEnumerable<PropertyInfo> object1PropertyInfos = ReflectionHelper.GetPropertyInfos(object1.GetType());
            IEnumerable<PropertyInfo> object2PropertyInfos = ReflectionHelper.GetPropertyInfos(object2.GetType());

            using (IEnumerator<PropertyInfo> propertyInfo1Enumerator = object1PropertyInfos.GetEnumerator(),
                                             propertyInfo2Enumerator = object2PropertyInfos.GetEnumerator())
            {
                while (propertyInfo1Enumerator.MoveNext() && propertyInfo2Enumerator.MoveNext())
                {
                    // Found IgnoreVarianceAttribute
                    if (ReflectionHelper.HasIgnoreVarianceAttribute(propertyInfo1Enumerator.Current) ||
                        ReflectionHelper.HasIgnoreVarianceAttribute(propertyInfo2Enumerator.Current))
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
    }
}
