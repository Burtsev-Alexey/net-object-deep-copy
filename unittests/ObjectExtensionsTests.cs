#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace unittests
{
    [TestClass]
    public class ObjectExtensionsTests
    {
        private class MySingleObject
        {
            public string One;
            private int two;

            public int Two
            {
                get { return two; }
                set { two = value; }
            }
        }

        private class MyNestedObject
        {
            public MySingleObject Single;
            public string Meta;
        }

        [TestMethod]
        public void Copy_XElementWithChildren()
        {
            XElement el = XElement.Parse(@"
                <root>
                    <child attrib='wow'>hi</child>
                    <child attrib='yeah'>hello</child>
                </root>");
            XElement copied = el.Copy();

            var children = copied.Elements("child").ToList();
            Assert.AreEqual(2, children.Count);
            Assert.AreEqual("wow", children[0].Attribute("attrib").Value);
            Assert.AreEqual("hi", children[0].Value);

            Assert.AreEqual("yeah", children[1].Attribute("attrib").Value);
            Assert.AreEqual("hello", children[1].Value);
        }

        [TestMethod]
        public void Copy_CopiesNestedObject()
        {
            MyNestedObject copied =
                new MyNestedObject() {Meta = "metadata", Single = new MySingleObject() {One = "single_one"}}.Copy();

            Assert.AreEqual("metadata", copied.Meta);
            Assert.AreEqual("single_one", copied.Single.One);
        }

        [TestMethod]
        public void Copy_CopiesEnumerables()
        {
            IList<MySingleObject> list = new List<MySingleObject>()
            {
                new MySingleObject() {One = "1"},
                new MySingleObject() {One = "2"}
            };
            IList<MySingleObject> copied = list.Copy();

            Assert.AreEqual(2, copied.Count);
            Assert.AreEqual("1", list[0].One);
            Assert.AreEqual("2", list[1].One);
        }

        [TestMethod]
        public void Copy_CopiesSingleObject()
        {
            MySingleObject copied = new MySingleObject() {One = "one", Two = 2}.Copy();

            Assert.AreEqual("one", copied.One);
            Assert.AreEqual(2, copied.Two);
        }

        [TestMethod]
        public void Copy_CopiesSingleBuiltInObjects()
        {
            Assert.AreEqual("hello there", "hello there".Copy());
            Assert.AreEqual(123, 123.Copy());
        }
    }
}