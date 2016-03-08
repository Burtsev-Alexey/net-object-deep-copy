using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ObjectCopy
{
    public static class ObjectCloneVerifierExtensions
    {
        public static bool IsPrimitive(this Type type)
        {
            if (type == typeof(String)) return true;
            return (type.IsValueType & type.IsPrimitive);
        }

        public static bool Verify(this Object originalObject, object copyObject)
        {
            return InternalVerify(originalObject, copyObject);
        }

        private static bool InternalVerify(Object originalObject, Object copyObject)
        {
            if (originalObject == null) return copyObject == null;

            var typeToReflect = originalObject.GetType();

            if (IsPrimitive(typeToReflect)) return true; //A little blind here, assumption is that primitives are fine already

            if (typeof(Delegate).IsAssignableFrom(typeToReflect)) return copyObject == null;

            if (typeToReflect.CustomAttributes.Any(x => x.AttributeType == typeof(IgnoreCopyAttribute))) 
                return copyObject == null;

            if (typeToReflect.CustomAttributes.Any(x => x.AttributeType == typeof(ShallowCloneAttribute)))
                return ReferenceEquals(originalObject, copyObject);


            if (typeToReflect.IsArray)
            {
                var arrayType = typeToReflect.GetElementType();
                if (IsPrimitive(arrayType) == false)
                {
                    Array clonedArray = (Array)copyObject;
                    Array originalArray = (Array)originalObject;

                    if (clonedArray.Length != originalArray.Length) return false;
                    var aggregateResult = true;

                    for (int i = 0; i < originalArray.Length; i++)
                    {
                        var res = InternalVerify(originalArray.GetValue(i), clonedArray.GetValue(i));
                        if (!res)
                        {
                            return false;
                        }
                    }
                }
            }

            if (ReferenceEquals(originalObject, copyObject)) return false;

            var state = IterateFields(originalObject, copyObject, typeToReflect);
            if (!state) return false;

            state = RecursiveCopyBaseTypePrivateFields(originalObject, copyObject, typeToReflect);
            return state;
        }

        private static bool RecursiveCopyBaseTypePrivateFields(object originalObject, object cloneObject, Type typeToReflect)
        {
            if (typeToReflect.BaseType != null)
            {
                RecursiveCopyBaseTypePrivateFields(originalObject, cloneObject, typeToReflect.BaseType);
                return IterateFields(originalObject, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
            }
            return true;
        }

        private static bool IterateFields(object originalObject, object cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
        {
            var aggregateState = true;
            foreach (FieldInfo fieldInfo in typeToReflect.GetFields(bindingFlags))
            {
                if (filter != null && filter(fieldInfo) == false) continue;
                if (IsPrimitive(fieldInfo.FieldType)) continue;

                var originalFieldValue = fieldInfo.GetValue(originalObject);
                var copyFieldValue = fieldInfo.GetValue(cloneObject);

                if (fieldInfo.CustomAttributes.Any(x => x.AttributeType == typeof(IgnoreCopyAttribute)))
                {
                    aggregateState &= copyFieldValue == null;
                    continue;
                }

                if (fieldInfo.CustomAttributes.Any(x => x.AttributeType == typeof(ShallowCloneAttribute)))
                {
                    aggregateState &= ReferenceEquals(originalFieldValue, copyFieldValue);
                    continue;
                }

                if (fieldInfo.IsBackingField())
                {

                    var property = fieldInfo.GetBackingFieldProperty(typeToReflect, bindingFlags);

                    if (property.CustomAttributes.Any(x => x.AttributeType == typeof(IgnoreCopyAttribute)))
                    {
                        aggregateState &= copyFieldValue == null;
                        continue;
                    }

                    if (property.CustomAttributes.Any(x => x.AttributeType == typeof(ShallowCloneAttribute)))
                    {
                        aggregateState &= ReferenceEquals(originalFieldValue, copyFieldValue);
                        continue;
                    }
                }

                var result = InternalVerify(originalFieldValue, copyFieldValue);

                if (!result)
                {
                    aggregateState = false;
                    break;
                }
            }
            return aggregateState;
        }
    }
}