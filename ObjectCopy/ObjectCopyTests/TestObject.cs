using System;
using System.Collections.Generic;
using ObjectCopy;

namespace ObjectCopyTests
{
    public class TestObject : ICloneable<TestObject>, IDisposable
    {
        public string StringProp { get; set; }
        private string StringProp2 { get; set; }
        public TestObject2 ObjectProp { get; set; }
        [ShallowClone]
        public TestObject2 ObjectProp2 { get; set; }
        public TestObject2[] ObjectCollection { get; set; }
        public List<TestObject2> ObjectCollection2 { get; set; }
        public Dictionary<string, TestObject2> DictionaryProp { get; set; }


        public TestObject()
        {
            this.StringProp = "One";
            this.StringProp2 = "Two";
            this.ObjectProp = new TestObject2();
            this.ObjectProp2 = new TestObject2();
            this.ObjectCollection = new TestObject2[] { new TestObject2(), new TestObject2(), new TestObject2() };
            this.ObjectCollection2 = new List<TestObject2>()
            {
                new TestObject2(),
                new TestObject2(),
                new TestObject2(),
                new TestObject2(),
            };
            this.DictionaryProp = new Dictionary<string, TestObject2>()
            {
                {"A",new TestObject2()},
                {"B",new TestObject2()},
                {"C",new TestObject2()},
                {"D",new TestObject2()},
            };
        }

        public TestObject Clone()
        {
            return this.Copy();
        }

        public void Dispose()
        {
        }
    }
}