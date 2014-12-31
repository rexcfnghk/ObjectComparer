using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;

namespace Voyagers.Utilities.ObjectComparer
{
    internal static class KeyPropertiesComparer
    {
        public static IEnumerable<ObjectVariance> GetSetDifferenceVariances(IQueryable query1,
                                                                            IQueryable query2,
                                                                            ObjectVariance parentVariance)
        {
            IQueryable keyQuery1 = query1.Select("it.Key");
            IQueryable keyQuery2 = query2.Select("it.Key");

            IEnumerable<object> keyList1 = from dynamic q in keyQuery1 select q;
            IEnumerable<object> keyList2 = from dynamic q in keyQuery2 select q;

            List<object> extraKeysIn1 = keyList1.Except(keyList2).ToList();
            List<object> extraKeysIn2 = keyList2.Except(keyList1).ToList();

            // Extra Keys in query1
            foreach (var extraKey in extraKeysIn1)
            {
                foreach (dynamic group1 in query1)
                {
                    if (group1.Count > 1)
                    {
                        throw new InvalidOperationException("The IEnumerable contains objects with the same key");
                    }

                    if (!Object.Equals(group1.Key, extraKey))
                    {
                        continue;
                    }

                    foreach (var object1 in group1.Value)
                    {
                        string propertyName = parentVariance != null
                                                  ? !String.IsNullOrEmpty(parentVariance.PropertyName)
                                                        ? parentVariance.PropertyName
                                                        : "IEnumerable 1"
                                                  : "IEnumerable 1";
                        yield return
                            new ObjectVariance(String.Format("Extra object in {0} with key {1}", propertyName, group1.Key.ToString()), object1, null, parentVariance);
                    }
                }
            }

            // Extra Keys in query2
            foreach (var extraKey in extraKeysIn2)
            {
                foreach (dynamic group2 in query2)
                {
                    if (group2.Count > 1)
                    {
                        throw new InvalidOperationException("The IEnumerable contains objects with the same key");
                    }

                    if (!Object.Equals(group2.Key, extraKey))
                    {
                        continue;
                    }

                    foreach (var object2 in group2.Value)
                    {
                        string propertyName = parentVariance != null
                                                  ? !String.IsNullOrEmpty(parentVariance.PropertyName)
                                                        ? parentVariance.PropertyName
                                                        : "IEnumerable 2"
                                                  : "IEnumerable 2";
                        yield return new ObjectVariance(String.Format("Extra object in {0} with key {1}", propertyName, group2.Key.ToString()), null, object2, parentVariance);
                    }
                }
            }
        }
    }
}
