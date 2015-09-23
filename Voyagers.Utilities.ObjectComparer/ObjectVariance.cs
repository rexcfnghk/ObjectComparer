using System;

namespace Voyagers.Utilities.ObjectComparer
{
    /// <summary>
    /// Immutable class that represents a variance in the values of the objects
    /// </summary>
    public class ObjectVariance : IEquatable<ObjectVariance>
    {
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
            PropertyName = propertyName;
            PropertyValue1 = propertyValue1;
            PropertyValue2 = propertyValue2;
            ParentVariance = parentVariance;
        }

        /// <summary>
        /// Property that returns the name of the property that is being compared
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Property that returns value 1 of the property that is being compared
        /// </summary>
        public object PropertyValue1 { get; }

        /// <summary>
        /// Property that returns value 2 of the property that is being compared
        /// </summary>
        public object PropertyValue2 { get; }

        /// <summary>
        /// Property that returns the recursive, outer variance that this variance arises from
        /// </summary>
        public ObjectVariance ParentVariance { get; }

        /// <summary>
        /// Overloaded == operator that compares the equality of two ObjectVariances
        /// </summary>
        /// <param name="left">Left ObjectVariance</param>
        /// <param name="right">Right ObjectVariance</param>
        /// <returns>True or False</returns>
        public static bool operator ==(ObjectVariance left, ObjectVariance right) => Equals(left, right);

        /// <summary>
        /// Overloaded != opeartor that compares the equality of two ObjectVariances
        /// </summary>
        /// <param name="left">Left ObjectVariance</param>
        /// <param name="right">Right ObjectVariance</param>
        /// <returns>True or False</returns>
        public static bool operator !=(ObjectVariance left, ObjectVariance right) => !Equals(left, right);

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

            return string.Equals(PropertyName, other.PropertyName, StringComparison.Ordinal) &&
                   Equals(PropertyValue1, other.PropertyValue1) &&
                   Equals(PropertyValue2, other.PropertyValue2) &&
                   Equals(ParentVariance, other.ParentVariance);
        }

        /// <summary>
        /// Override of Object.Equals(object)
        /// </summary>
        /// <param name="obj">Object that is being compared for equality</param>
        /// <returns>True or False</returns>
        public override bool Equals(object obj) => Equals(obj as ObjectVariance);

        /// <summary>
        /// GetHashCode implementation credits to Jon Skeet: http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
        /// </summary>
        /// <returns>Hashcode generated from the fields of ObjectVariance</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = (hashCode * 23) + (PropertyName?.GetHashCode() ?? 0);
                hashCode = (hashCode * 23) + (PropertyValue1?.GetHashCode() ?? 0);
                hashCode = (hashCode * 23) + (PropertyValue2?.GetHashCode() ?? 0);
                hashCode = (hashCode * 23) + (ParentVariance?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}