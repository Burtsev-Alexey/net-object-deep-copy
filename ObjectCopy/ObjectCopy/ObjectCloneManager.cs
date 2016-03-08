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
    public class ObjectCloneManager
    {
        private Func<object, object> CloneMethod;
        private Dictionary<Type, FieldInfo[]> fieldsRequiringDeepClone;

        public ObjectCloneManager()
        {
            MethodInfo cloneMethod = typeof(Object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);
            var p1 = Expression.Parameter(typeof(object));
            var body = Expression.Call(p1, cloneMethod);
            CloneMethod = Expression.Lambda<Func<object, object>>(body, p1).Compile();
            fieldsRequiringDeepClone = new Dictionary<Type, FieldInfo[]>();
        }

        public Object Copy(Object originalObject)
        {
            return this.InternalCopy(originalObject, new Dictionary<Object, Object>(new ReferenceEqualityComparer()),true);
        }

        private Object InternalCopy(Object originalObject, IDictionary<Object, Object> visited, bool checkObjectGraph)
        {
            if (originalObject == null) return null;
            var typeToReflect = originalObject.GetType();

            if (typeToReflect.IsPrimitive())
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

            if (checkObjectGraph && visited.ContainsKey(originalObject)) return visited[originalObject];
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
                if (checkObjectGraph) visited.Add(originalObject, cloneObject);

                if (arrayType.IsPrimitive())
                {
                    //ignore array of primitive, shallow copy will suffic
                }
                else if (typeToReflect.IsPrimitive() == false)
                {
                    clonedArray.ForEach((array, indices) => array.SetValue(InternalCopy(originalArray.GetValue(indices), visited, !arrayType.IsValueType), indices));
                }
                else
                {
                    Array.Copy(originalArray, clonedArray, clonedArray.Length);
                }

            }
            else
            {
                cloneObject = CloneMethod.Invoke(originalObject);
                if(checkObjectGraph)visited.Add(originalObject, cloneObject);
            }

            CopyFields(originalObject, visited, cloneObject, typeToReflect);
            RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect);
            return cloneObject;
        }

        private void RecursiveCopyBaseTypePrivateFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect)
        {
            if (typeToReflect.BaseType != null)
            {
                RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
                CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
            }
        }

        private FieldInfo[] CachedFieldsRequiringDeepClone(Type typeToReflect, object cloneObject)
        {
            FieldInfo[] result;

            if (!fieldsRequiringDeepClone.TryGetValue(typeToReflect, out result))
            {
                result = FieldsRequiringDeepClone(typeToReflect, cloneObject).ToArray();
                fieldsRequiringDeepClone[typeToReflect] = result;
            }

            return result;
        }

        private IEnumerable<FieldInfo> FieldsRequiringDeepClone(Type typeToReflect, object cloneObject)
        {
            foreach (FieldInfo fieldInfo in typeToReflect.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy))
            {
                if (fieldInfo.FieldType.IsPrimitive())
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
                    if (fieldInfo.FieldType.IsPrimitive())
                    {
                        UndoShallowCopyOfPrimitiveTypesWithIgnoreCopyAttribute(fieldInfo, cloneObject, typeToReflect, BindingFlags.Instance | BindingFlags.NonPublic);
                        continue;
                    }
                    yield return fieldInfo;
                }
            }
        }

        private void UndoShallowCopyOfPrimitiveTypesWithIgnoreCopyAttribute(FieldInfo fieldInfo, object cloneObject, Type typeToReflect, BindingFlags bindingFlags)
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

        private void CopyFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
        {
            foreach (FieldInfo fieldInfo in CachedFieldsRequiringDeepClone(typeToReflect, cloneObject))
            {
                if (filter != null && filter(fieldInfo) == false) continue;
                if (fieldInfo.FieldType.IsPrimitive())
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
                var clonedFieldValue = InternalCopy(originalFieldValue, visited, !fieldInfo.FieldType.IsValueType);
                fieldInfo.SetValue(cloneObject, clonedFieldValue);
            }
        }
    }
}