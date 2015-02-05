using System;

namespace Voyagers.Utilities.ObjectComparer
{
    /// <summary>
    /// Immutable class that represents a variance in the values of the objects
    /// </summary>
    public class ObjectVariance : IObjectVariance
    {
        private readonly string _propertyName;
        private readonly object _propertyValue1;
        private readonly object _propertyValue2;
        private readonly ObjectVariance _parentVariance;

        /// <summary>
        /// Constructs an instance of ObjectVariance.
        /// </summary>
        /// <param name="propertyName">Name of the property being compared</param>
        /// <param name="propertyValue1">Value 1 of the property being compared</param>
        /// <param name="propertyValue2">Value 2 of the property being compared</param>
        /// <param name="parentVariance">The recursive, outer variance that this variance arises from</param>
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

        /// <summary>
        /// Property that returns the name of the property that is being compared
        /// </summary>
        public string PropertyName
        {
            get { return _propertyName; }
        }

        /// <summary>
        /// Property that returns value 1 of the property that is being compared
        /// </summary>
        public object PropertyValue1
        {
            get { return _propertyValue1; }
        }

        /// <summary>
        /// Property that returns value 2 of the property that is being compared
        /// </summary>
        public object PropertyValue2
        {
            get { return _propertyValue2; }
        }

        /// <summary>
        /// Property that returns the recursive, outer variance that this variance arises from
        /// </summary>
        public ObjectVariance ParentVariance
        {
            get { return _parentVariance; }
        }

        /// <summary>
        /// Overloaded == operator that compares the equality of two ObjectVariances
        /// </summary>
        /// <param name="left">Left ObjectVariance</param>
        /// <param name="right">Right ObjectVariance</param>
        /// <returns>True or False</returns>
        public static bool operator ==(ObjectVariance left, ObjectVariance right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Overloaded != opeartor that compares the equality of two ObjectVariances
        /// </summary>
        /// <param name="left">Left ObjectVariance</param>
        /// <param name="right">Right ObjectVariance</param>
        /// <returns>True or False</returns>
        public static bool operator !=(ObjectVariance left, ObjectVariance right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// IEquatable&lt;ObjectVariance&gt;.Equals(ObjectVariance) implementaion
        /// </summary>
        /// <param name="other">Other ObjectVariance</param>
        /// <returns>True or False</returns>
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

        /// <summary>
        /// Override of Object.Equals(object)
        /// </summary>
        /// <param name="obj">Object that is being compared for equality</param>
        /// <returns>True or False</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return ReferenceEquals(this, obj) || Equals(obj as ObjectVariance);
        }

        /// <summary>
        /// GetHashCode implementation credits to Jon Skeet: http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
        /// </summary>
        /// <returns>Hashcode generated from the fields of ObjectVariance</returns>
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