using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;

namespace ObjectCopy
{
    /// <summary>
    /// Deep clone objectextensions
    /// </summary>
    public static class ObjectCloneExtensions
    {
        private static Func<object, object> CloneMethod;

        static ObjectCloneExtensions()
        {
            MethodInfo cloneMethod = typeof(Object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);
            var p1 = Expression.Parameter(typeof(object));
            var body = Expression.Call(p1, cloneMethod);
            CloneMethod = Expression.Lambda<Func<object, object>>(body, p1).Compile();
        }

        private static bool IsPrimitive(Type type)
        {
            if (type.IsValueType && type.IsPrimitive) return true;
            if (type == typeof(String)) return true;
            if (type == typeof(Decimal)) return true;
            if (type == typeof(DateTime)) return true;
            return false;
        }


        public static Object Copy(this Object originalObject)
        {
            return InternalCopy(originalObject, new Dictionary<Object, Object>(new ReferenceEqualityComparer()));
        }

        private static Object InternalCopy(Object originalObject, IDictionary<Object, Object> visited)
        {
            if (originalObject == null) return null;
            var typeToReflect = originalObject.GetType();

            if (IsPrimitive(typeToReflect))
            {
                if (typeToReflect.CustomAttributes.Any(x => x.AttributeType == typeof(IgnoreCopyAttribute)))
                {
                    return null;
                }
                else
                {
                    return originalObject;
                }
            }

            if (typeof(XElement).IsAssignableFrom(typeToReflect)) return new XElement(originalObject as XElement);

            if (visited.ContainsKey(originalObject)) return visited[originalObject];
            if (typeof(Delegate).IsAssignableFrom(typeToReflect)) return null;

            if (typeToReflect.CustomAttributes.Any(x => x.AttributeType == typeof(IgnoreCopyAttribute)))
                return null;

            if (typeToReflect.CustomAttributes.Any(x => x.AttributeType == typeof(ShallowCloneAttribute)))
                return originalObject;

            object cloneObject = null;

            if (typeToReflect.IsArray)
            {
                var arrayType = typeToReflect.GetElementType();

                Array originalArray = (Array)originalObject;
                Array clonedArray = Array.CreateInstance(arrayType, originalArray.Length);
                cloneObject = clonedArray;
                visited.Add(originalObject, cloneObject);

                if (IsPrimitive(arrayType) == false)
                {
                    clonedArray.ForEach((array, indices) => array.SetValue(InternalCopy(originalArray.GetValue(indices), visited), indices));
                }
                else
                {
                    Array.Copy(originalArray, clonedArray, clonedArray.Length);
                }

            }
            else
            {
                cloneObject = CloneMethod.Invoke(originalObject);
                visited.Add(originalObject, cloneObject);
            }

            CopyFields(originalObject, visited, cloneObject, typeToReflect);
            RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect);
            return cloneObject;
        }

        private static void RecursiveCopyBaseTypePrivateFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect)
        {
            if (typeToReflect.BaseType != null)
            {
                RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
                CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
            }
        }

        private static IEnumerable<FieldInfo> FieldsRequiringDeepClone(Type typeToReflect, object cloneObject)
        {
            foreach (FieldInfo fieldInfo in typeToReflect.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy))
            {
                if (IsPrimitive(fieldInfo.FieldType))
                {
                    UndoShallowCopyOfPrimitiveTypesWithIgnoreCopyAttribute(fieldInfo, cloneObject, typeToReflect, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                    continue;
                }
                yield return fieldInfo;
            }

            while (typeToReflect.BaseType != null)
            {
                typeToReflect = typeToReflect.BaseType;

                foreach (FieldInfo fieldInfo in typeToReflect.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    if (!fieldInfo.IsPrivate) continue;
                    if (IsPrimitive(fieldInfo.FieldType))
                    {
                        UndoShallowCopyOfPrimitiveTypesWithIgnoreCopyAttribute(fieldInfo, cloneObject, typeToReflect, BindingFlags.Instance | BindingFlags.NonPublic);
                        continue;
                    }
                    yield return fieldInfo;
                }
            }
        }

        private static void UndoShallowCopyOfPrimitiveTypesWithIgnoreCopyAttribute(FieldInfo fieldInfo,object cloneObject,Type typeToReflect,BindingFlags bindingFlags)
        {
            if (fieldInfo.CustomAttributes.Any(x => x.AttributeType == typeof(IgnoreCopyAttribute)))
            {
                if (fieldInfo.FieldType == typeof(string))
                {
                    fieldInfo.SetValue(cloneObject, null);
                }
                else
                {
                    fieldInfo.SetValue(cloneObject, Activator.CreateInstance(fieldInfo.FieldType));
                }
            }

            if (fieldInfo.IsBackingField())
            {
                var property = fieldInfo.GetBackingFieldProperty(typeToReflect, bindingFlags);
                if (property.CustomAttributes.Any(x => x.AttributeType == typeof(IgnoreCopyAttribute)))
                {
                    if (fieldInfo.FieldType == typeof(string))
                    {
                        fieldInfo.SetValue(cloneObject, null);
                    }
                    else
                    {
                        fieldInfo.SetValue(cloneObject, Activator.CreateInstance(fieldInfo.FieldType));
                    }
                }
            }
        }

        private static void CopyFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
        {
            foreach (FieldInfo fieldInfo in FieldsRequiringDeepClone(typeToReflect,cloneObject))
            {
                if (filter != null && filter(fieldInfo) == false) continue;
                if (IsPrimitive(fieldInfo.FieldType))
                {
                 

                    continue;
                }

                if (fieldInfo.CustomAttributes.Any(x => x.AttributeType == typeof(IgnoreCopyAttribute)))
                {
                    fieldInfo.SetValue(cloneObject, null);
                    continue;
                }

                if (fieldInfo.CustomAttributes.Any(x => x.AttributeType == typeof(ShallowCloneAttribute)))
                    continue;

                if (fieldInfo.IsBackingField())
                {
                    var property = fieldInfo.GetBackingFieldProperty(typeToReflect, bindingFlags);
                    if (property.CustomAttributes.Any(x => x.AttributeType == typeof(IgnoreCopyAttribute)))
                    {
                        fieldInfo.SetValue(cloneObject, null);
                        continue;
                    }
                    if (property.CustomAttributes.Any(x => x.AttributeType == typeof(ShallowCloneAttribute))) continue;
                }

                var originalFieldValue = fieldInfo.GetValue(originalObject);
                var clonedFieldValue = InternalCopy(originalFieldValue, visited);
                fieldInfo.SetValue(cloneObject, clonedFieldValue);
            }
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
            return (T)Copy((Object)original);
        }
    }
}
