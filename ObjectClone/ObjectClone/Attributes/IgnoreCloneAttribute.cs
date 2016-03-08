using System;

namespace ObjectClone.Attributes
{
    /// <summary>
    ///     Marks an item to be ignored during cloning, will remain null(reference types)/default(value types)
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    public class IgnoreCloneAttribute : Attribute
    {
    }
}