using System;
using System.Collections.Generic;
using ObjectCopy;

namespace ObjectCopyTests
{
    [IgnoreCopy]
    public class TestObject3
    {
        public string One { get; set; }

        public TestObject3()
        {
            this.One = "One";
        }
    }

    public class TestObject : ICloneable<TestObject>, IDisposable
    {
        public string StringProp { get; set; }

        [IgnoreCopy]
        private int One { get; set; }
        [IgnoreCopy]
        private string Other { get; set; }
        private string StringProp2 { get; set; }

        public TestObject2 ObjectProp { get; set; }
        [ShallowClone]
        public TestObject2 ObjectProp2 { get; set; }

        [IgnoreCopy]
        public TestObject2 ObjectProp3 { get; set; }

        public TestObject3[] ObjectCollection3 { get; set; }

        [IgnoreCopy]
        public TestObject3[] ObjectCollection4 { get; set; }


        public TestObject2[] ObjectCollection { get; set; }
        public List<TestObject2> ObjectCollection2 { get; set; }
        public Dictionary<string, TestObject2> DictionaryProp { get; set; }


        public TestObject()
        {
            this.Other = "Hello";
            this.StringProp = "One";
            this.StringProp2 = "Two";
            this.One = 1;
            this.ObjectProp = new TestObject2();
            this.ObjectProp2 = new TestObject2();
            this.ObjectProp3 = new TestObject2();

            this.ObjectCollection = new TestObject2[] { new TestObject2(), new TestObject2(), new TestObject2() };
            this.ObjectCollection3 = new TestObject3[] { new TestObject3(), new TestObject3(), new TestObject3() };
            this.ObjectCollection4 = new TestObject3[] { new TestObject3(), new TestObject3(), new TestObject3() };
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