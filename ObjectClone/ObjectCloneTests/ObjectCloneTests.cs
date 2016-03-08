using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using ObjectClone.Cloning;
using ObjectClone.Helpers;

namespace ObjectCloneTests
{
    public class ObjectCloneTests
    {
        #region Fields

        private TestClass clonedObject;

        private TestClass originalObject;

        #endregion

        [SetUp]
        public void SetupTest()
        {
            this.originalObject = new TestClass();
            this.clonedObject = this.originalObject.Clone();
        }

        [TearDown]
        public void TearDownTest()
        {
            this.originalObject.Dispose();
            this.clonedObject.Dispose();
        }

        #region Bug Tests

        [Test]
        public void TestBugWithRecursiveCloneOfArraySelfReference()
        {
            //Reported by Jean-Paul Mikkers
            var test = new object[1];
            test[0] = test;
            test.Copy(); // CRASH - fixed
        }

        #endregion

        #region Algorithm verified tests

        [Test]
        [UseReporter(typeof (DiffReporter))]
        public void TestObjectDeepCloneToBaseline() //Ensure all required fields, collections items etc
        {
            //Ensure all field have been copied by using an approved baseline
            //This is useful in testing breaking changes
            Approvals.Verify(JsonConvert.SerializeObject(this.clonedObject));
        }

        [Test]
        public void TestObjectCloneEnsureIsDeep() //Verify that all objects are deep copied
        {
            Assert.IsTrue(this.originalObject.DeepCloneIsRespected(this.clonedObject));

            this.clonedObject.PropertySeventeen["A"] = this.originalObject.PropertySeventeen["A"];

            Assert.IsFalse(this.originalObject.DeepCloneIsRespected(this.clonedObject));
            Assert.IsFalse(this.originalObject.DeepCloneIsRespected(this.originalObject));
        }

        #endregion

        #region "Jean-Paul Mikkers Tests - Manual"

        private class MySingleObject
        {
            #region Fields

            public string One;

            #endregion

            #region Fields

            #endregion

            #region Properties

            public int Two { get; set; }

            #endregion
        }

        private class MyNestedObject
        {
            #region Fields

            public string Meta;

            #endregion

            #region Fields

            public MySingleObject Single;

            #endregion
        }

        [Test]
        public void Copy_XElementWithChildren()
        {
            XElement el = XElement.Parse(@"
                <root>
                    <child attrib='wow'>hi</child>
                    <child attrib='yeah'>hello</child>
                </root>");
            XElement copied = el.Copy();

            List<XElement> children = copied.Elements("child").ToList();
            Assert.AreEqual(2, children.Count);
            Assert.AreEqual("wow", children[0].Attribute("attrib").Value);
            Assert.AreEqual("hi", children[0].Value);

            Assert.AreEqual("yeah", children[1].Attribute("attrib").Value);
            Assert.AreEqual("hello", children[1].Value);
        }

        [Test]
        public void Copy_CopiesNestedObject()
        {
            MyNestedObject copied =
                new MyNestedObject {Meta = "metadata", Single = new MySingleObject {One = "single_one"}}.Copy();

            Assert.AreEqual("metadata", copied.Meta);
            Assert.AreEqual("single_one", copied.Single.One);
        }

        [Test]
        public void Copy_CopiesEnumerables()
        {
            IList<MySingleObject> list = new List<MySingleObject>
            {
                new MySingleObject {One = "1"},
                new MySingleObject {One = "2"}
            };
            IList<MySingleObject> copied = list.Copy();

            Assert.AreEqual(2, copied.Count);
            Assert.AreEqual("1", list[0].One);
            Assert.AreEqual("2", list[1].One);
        }

        [Test]
        public void Copy_CopiesSingleObject()
        {
            MySingleObject copied = new MySingleObject {One = "one", Two = 2}.Copy();

            Assert.AreEqual("one", copied.One);
            Assert.AreEqual(2, copied.Two);
        }

        private class OverriddenHash
        {
            public override int GetHashCode()
            {
                return 42;
            }
        }

        [Test]
        public void ReferenceEqualityComparerShouldNotUseOverriddenHash()
        {
            var t = new OverriddenHash();
            var equalityComparer = new ReferenceEqualityComparer();
            Assert.AreNotEqual(42, equalityComparer.GetHashCode(t));
            Assert.AreEqual(equalityComparer.GetHashCode(t), RuntimeHelpers.GetHashCode(t));
        }

        [Test]
        public void Copy_CopiesSingleBuiltInObjects()
        {
            Assert.AreEqual("hello there", "hello there".Copy());
            Assert.AreEqual(123, 123.Copy());
        }

        [Test]
        public void Copy_CopiesSelfReferencingArray()
        {
            var arr = new object[1];
            arr[0] = arr;
            object[] copy = arr.Copy();
            Assert.IsTrue(ReferenceEquals(copy, copy[0]));
        }

        #endregion
    }
}