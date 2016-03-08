using System;

namespace ObjectClone.Attributes
{
    /// <summary>
    ///     Marks an item to be shallow copied during cloning
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    public sealed class ShallowCloneAttribute : Attribute
    {
    }
}