using System;

namespace Voyagers.Utilities.ObjectComparer
{
    /// <summary>
    /// ObjectVariance that takes the form of <c>Object1.PropertyName != Object2.PropertyName</c>
    /// </summary>
    public class ObjectVariance : IEquatable<ObjectVariance>
    {
        private readonly string _propertyName;
        private readonly object _propertyValue1;
        private readonly object _propertyValue2;
        private readonly ObjectVariance _parentVariance;

        public ObjectVariance(string propertyName,
                              object propertyValue1,
                              object propertyValue2,
                              ObjectVariance parentVariance)
        {
            _propertyName = propertyName;
            _propertyValue1 = propertyValue1;
            _propertyValue2 = propertyValue2;
            _parentVariance = parentVariance;
        }

        public string PropertyName
        {
            get { return _propertyName; }
        }

        public object PropertyValue1
        {
            get { return _propertyValue1; }
        }

        public object PropertyValue2
        {
            get { return _propertyValue2; }
        }

        public ObjectVariance ParentVariance
        {
            get { return _parentVariance; }
        }

        public static bool operator ==(ObjectVariance left, ObjectVariance right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ObjectVariance left, ObjectVariance right)
        {
            return !Equals(left, right);
        }

        public bool Equals(ObjectVariance other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return String.Equals(_propertyName, other._propertyName) && Equals(_propertyValue1, other._propertyValue1) &&
                   Equals(_propertyValue2, other._propertyValue2) &&
                   Equals(_parentVariance, other._parentVariance);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() && Equals((ObjectVariance)obj);
        }

        /// <summary>
        /// GetHashCode implementation credits to Jon Skeet: http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = (hashCode * 23) + (_propertyName != null ? _propertyName.GetHashCode() : 0);
                hashCode = (hashCode * 23) + (_propertyValue1 != null ? _propertyValue1.GetHashCode() : 0);
                hashCode = (hashCode * 23) + (_propertyValue2 != null ? _propertyValue2.GetHashCode() : 0);
                hashCode = (hashCode * 23) + (_parentVariance != null ? _parentVariance.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}