using System;

namespace ObjectCopy
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class IgnoreCopyAttribute : Attribute
    {
        
    }
}