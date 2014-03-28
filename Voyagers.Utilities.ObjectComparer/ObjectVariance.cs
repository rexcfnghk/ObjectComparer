using System;

namespace Voyagers.Utilities.ObjectComparer
{
    public class ObjectVariance : IEquatable<ObjectVariance>
    {
        private readonly string _propertyName;
        private readonly object _value1;
        private readonly object _value2;
        private readonly int _level;
        private readonly ObjectVariance _parentVariance;

        public ObjectVariance(string propertyName,
                              object value1,
                              object value2,
                              int level,
                              ObjectVariance parentVariance)
        {
            _propertyName = propertyName;
            _value1 = value1;
            _value2 = value2;
            _level = level;
            _parentVariance = parentVariance;
        }

        public string PropertyName
        {
            get { return _propertyName; }
        }

        public object Value1
        {
            get { return _value1; }
        }

        public object Value2
        {
            get { return _value2; }
        }

        public int Level
        {
            get { return _level; }
        }

        public ObjectVariance ParentVariance
        {
            get { return _parentVariance; }
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

            return String.Equals(_propertyName, other._propertyName) && Equals(_value1, other._value1) &&
                   Equals(_value2, other._value2) && _level == other._level &&
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

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (_propertyName != null ? _propertyName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_value1 != null ? _value1.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_value2 != null ? _value2.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ _level;
                hashCode = (hashCode * 397) ^ (_parentVariance != null ? _parentVariance.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(ObjectVariance left, ObjectVariance right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ObjectVariance left, ObjectVariance right)
        {
            return !Equals(left, right);
        }
    }
}