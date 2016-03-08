using System;
using System.Reflection;

namespace ObjectCopy
{
    public static class ObjectCloneExtensions
    {
        public static bool IsPrimitive(this Type type)
        {
            if (type.IsValueType && type.IsPrimitive) return true;
            if (type == typeof(String)) return true;
            if (type == typeof(Decimal)) return true;
            if (type == typeof(DateTime)) return true;
            return false;
        }

        public static PropertyInfo GetBackingFieldProperty(this FieldInfo fieldInfo, Type typeToReflect, BindingFlags bindingFlags)
        {
            return typeToReflect.GetProperty(fieldInfo.Name.Substring(1, fieldInfo.Name.IndexOf("k__", StringComparison.Ordinal) - 2), bindingFlags);
        }

        public static bool IsBackingField(this FieldInfo fieldInfo)
        {
            return fieldInfo.Name.Contains("k__BackingField");
        }

        public static T Copy<T>(this T original)
        {
            return (T)new ObjectCloneManager().Copy((Object)original);
        }
    }
}