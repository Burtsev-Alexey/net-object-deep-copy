using System;
using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using ObjectCopy;

namespace ObjectCopyTests
{
    public class ObjectCopyTests
    {
        private TestObject originalObject;
        private TestObject clonedObject;

        [SetUp]
        public void SetupTest()
        {
            this.originalObject = new TestObject();
            this.clonedObject = this.originalObject.Clone();
        }

        [TearDown]
        public void TearDownTest()
        {
            this.originalObject.Dispose();
            this.clonedObject.Dispose();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TestObjectDeepCopyToBaseline() //Ensure all required fields, collections items etc
        {
            //Ensure all field have been copied by using an approved baseline
            //This is useful in testing breaking changes
            Approvals.Verify(JsonConvert.SerializeObject(this.clonedObject));
        }

        [Test]
        public void TestObjectDeepCopyEnsureIsDeepCopy()//Verify that all objects are deep copied
        {
            var cloneObjectsAreUnique = this.originalObject.Verify(this.clonedObject);

            //this.clonedObject.DictionaryProp["A"] = this.originalObject.DictionaryProp["A"];//force ensure that collections are checked
            var cloneObjectsAreUnique2 = this.originalObject.Verify(this.clonedObject);
            var sameObjectNotUnique = this.originalObject.Verify(this.originalObject);
        }
    }
}
