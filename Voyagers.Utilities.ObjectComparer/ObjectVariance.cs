using System;

namespace Voyagers.Utilities.ObjectComparer
{
    public struct ObjectVariance : IEquatable<ObjectVariance>
    {
        private readonly string _propertyName;
        private readonly object _value1;
        private readonly object _value2;
        private readonly int _level;
        private readonly object _parentReference1;
        private readonly object _parentReference2;

        public ObjectVariance(string propertyName,
                              object value1,
                              object value2,
                              int level,
                              object parentReference1,
                              object parentReference2)
        {
            _propertyName = propertyName;
            _value1 = value1;
            _value2 = value2;
            _level = level;
            _parentReference1 = parentReference1;
            _parentReference2 = parentReference2;
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

        public object ParentReference1
        {
            get { return _parentReference1; }
        }

        public object ParentReference2
        {
            get { return _parentReference2; }
        }

        public static bool operator ==(ObjectVariance left, ObjectVariance right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ObjectVariance left, ObjectVariance right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is ObjectVariance && Equals((ObjectVariance)obj);
        }

        public bool Equals(ObjectVariance other)
        {
            return string.Equals(_propertyName, other._propertyName) && Equals(_value1, other._value1) &&
                   Equals(_value2, other._value2) && _level == other._level &&
                   Equals(_parentReference1, other._parentReference1) &&
                   Equals(_parentReference2, other._parentReference2);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (_propertyName != null ? _propertyName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_value1 != null ? _value1.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_value2 != null ? _value2.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ _level;
                hashCode = (hashCode * 397) ^ (_parentReference1 != null ? _parentReference1.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_parentReference2 != null ? _parentReference2.GetHashCode() : 0);
                return hashCode;
            }
        }

    }
}