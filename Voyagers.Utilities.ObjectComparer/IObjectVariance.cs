using System;

namespace Voyagers.Utilities.ObjectComparer
{
    public interface IObjectVariance : IEquatable<ObjectVariance>
    {
        /// <summary>
        /// Property that returns the name of the property that is being compared
        /// </summary>
        string PropertyName { get; }

        /// <summary>
        /// Property that returns value 1 of the property that is being compared
        /// </summary>
        object PropertyValue1 { get; }

        /// <summary>
        /// Property that returns value 2 of the property that is being compared
        /// </summary>
        object PropertyValue2 { get; }

        /// <summary>
        /// Property that returns the recursive, outer variance that this variance arises from
        /// </summary>
        ObjectVariance ParentVariance { get; }
    }
}