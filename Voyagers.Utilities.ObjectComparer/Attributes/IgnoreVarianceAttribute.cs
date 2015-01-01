using System;

namespace Voyagers.Utilities.ObjectComparer.Attributes
{
    /// <summary>
    /// Attribute that instructs <c>ObjectComparer</c> to ignore variances on a property or the whole class 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = false)]
    public sealed class IgnoreVarianceAttribute : Attribute
    {
    }
}
