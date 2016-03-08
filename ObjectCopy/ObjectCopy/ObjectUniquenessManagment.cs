using System.Runtime.Serialization;

namespace ObjectCopyTests
{
    public class ObjectUniquenessManagment<T> where T : class
    {
        private readonly ObjectIDGenerator objectIdGenerator;

        public ObjectUniquenessManagment()
        {
            this.objectIdGenerator = new ObjectIDGenerator();
        }

        /// <summary>
        /// Determines whether [is first encounter].
        /// Useful when you want to know if you have come across this object before without keeping references manually
        /// Observer nUnitUtilies.TestDeepClone for good application of this
        /// </summary>
        /// <returns></returns>
        public bool IsFirstEncounter(T targetObject)
        {
            bool isFirst;
            this.objectIdGenerator.GetId(targetObject, out isFirst);
            return isFirst;
        }
    }
}