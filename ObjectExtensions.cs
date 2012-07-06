using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace System
{
    public static class ObjectExtensions
    {
        private static readonly MethodInfo CloneMethod = typeof(Object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

        public static Object CreateInstance(Type type)
        {
            var instance = FormatterServices.GetUninitializedObject(type);
            return instance;
        }

        public static T Clone<T>(this T obj)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(memoryStream, obj);
                memoryStream.Position = 0;
                var copy = serializer.Deserialize(memoryStream);
                return (T)copy;
            }
        }

        public static bool IsPrimitive(this Type type)
        {
            return ((type.IsValueType | type == typeof(String)) & type.IsPrimitive);
        }

        public static Object Copy(this Object originalObject)
        {
            return InternalCopy(originalObject, new Dictionary<Object, Object>(new ReferenceEqualityComparer()));
        }
        private static Object InternalCopy(Object originalObject, IDictionary<Object, Object> visited)
        {
            if (originalObject == null) return null;
            var originalObjectType = originalObject.GetType();
            if (IsPrimitive(originalObjectType)) return originalObject;
            if (visited.ContainsKey(originalObject)) return visited[originalObject];
            var cloneObject = CloneMethod.Invoke(originalObject, null);
            visited.Add(originalObject, cloneObject);

            foreach (FieldInfo fieldInfo in originalObjectType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy))
            {
                if (IsPrimitive(fieldInfo.FieldType)) continue;
                var originalFieldValue = fieldInfo.GetValue(originalObject);
                var clonedFieldValue = originalFieldValue == null ? null : InternalCopy(originalFieldValue, visited);
                fieldInfo.SetValue(cloneObject, clonedFieldValue);
                if (clonedFieldValue == null) continue;
                if (fieldInfo.FieldType.IsArray)
                {
                    var arrayType = fieldInfo.FieldType.GetElementType();
                    if (IsPrimitive(arrayType)) continue;
                    Array clonedArray = (Array)clonedFieldValue;
                    for (long i = 0; i < clonedArray.LongLength; i++) clonedArray.SetValue(InternalCopy(clonedArray.GetValue(i), visited), i);
                }
            }
            return cloneObject;
        }
        public static T Copy<T>(this T original)
        {
            return (T)Copy((Object)original);
        }
    }
	
	public class ReferenceEqualityComparer : EqualityComparer<Object>
    {
        public override bool Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }

        public override int GetHashCode(object obj)
        {
            if (obj == null) return 0;
            return obj.GetHashCode();
        }
    }
}
