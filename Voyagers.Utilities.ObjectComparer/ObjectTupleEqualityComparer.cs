using System;
using System.Collections.Generic;

namespace Voyagers.Utilities.ObjectComparer
{
    internal class ObjectTupleEqualityComparer : IEqualityComparer<Tuple<object, object>>
    {
        public bool Equals(Tuple<object, object> x, Tuple<object, object> y)
        {
            return ReferenceEquals(x.Item1, y.Item1) && ReferenceEquals(x.Item2, y.Item2);
        }

        public int GetHashCode(Tuple<object, object> obj)
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = (hashCode * 23) + (obj.Item1 == null ? 0 : obj.Item1.GetHashCode());
                hashCode = (hashCode * 23) + (obj.Item2 == null ? 0 : obj.Item2.GetHashCode());
                return hashCode;
            }
        }
    }
}
