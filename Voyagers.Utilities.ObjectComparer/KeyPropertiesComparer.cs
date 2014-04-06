using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using System.Threading.Tasks;

namespace Voyagers.Utilities.ObjectComparer
{
    public static class KeyPropertiesComparer
    {
        public static IEnumerable<ObjectVariance> GetSetDifferenceVariances(IQueryable query1,
                                                                            IQueryable query2,
                                                                            ObjectVariance parentVariance)
        {
            IQueryable keyQuery1 = query1.Select("it.Key");
            IQueryable keyQuery2 = query2.Select("it.Key");

            IEnumerable<object> list1 = from dynamic q in keyQuery1 select q;
            IEnumerable<object> list2 = from dynamic q in keyQuery2 select q;

            List<object> extraKeysIn1 = list1.Except(list2).ToList();
            List<object> extraKeysIn2 = list2.Except(list1).ToList();

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
                        yield return
                            new ObjectVariance("Extra object in IEnumerable 1", object1, null, parentVariance);
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
                        yield return new ObjectVariance("Extra object in IEnumerable 2", null, object2, parentVariance);
                    }
                }
            }
        }
    }
}
