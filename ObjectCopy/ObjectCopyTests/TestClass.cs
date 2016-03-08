using System;
using System.Collections.Generic;
using ObjectClone.Attributes;
using ObjectClone.Helpers;

namespace ObjectCloneTests
{
    public class TestClass : ICloneable<TestClass>, IDisposable
    {
        #region Ctors

        public TestClass()
        {
            this.PropertyOne = 500;

            this.PropertyTwo = "TestClass.PropertyTwo";

            this.PropertyThree = 600;

            this.PropertyFour = new DateTime(1989, 1, 9);

            this.PropertyFive = 700;

            this.PropertySix = 800;

            this.PropertySeven = new DateTime(1992, 3, 1);

            this.PropertyEight = "TestClass.PropertyEight";

            this.PropertyNine = "TestClass.PropertyNine";

            this.PropertyTen = new TestClass2();

            this.PropertyEleven = new TestClass2();

            this.PropertyTwelve = new TestClass2();

            this.PropertyThirteen = new[]
            {
                new TestClass3(),
                new TestClass3(),
                new TestClass3(),
                new TestClass3(),
                new TestClass3()
            };

            this.PropertyFourteen = new[]
            {
                new TestClass3(),
                new TestClass3(),
                new TestClass3(),
                new TestClass3(),
                new TestClass3()
            };

            this.PropertyFifteen = new[]
            {
                new TestClass2(),
                new TestClass2(),
                new TestClass2(),
                new TestClass2(),
                new TestClass2()
            };

            this.PropertySixteen = new List<TestClass2>
            {
                new TestClass2(),
                new TestClass2(),
                new TestClass2(),
                new TestClass2(),
                new TestClass2()
            };

            this.PropertySeventeen = new Dictionary<string, TestClass2>
            {
                {"A", new TestClass2()},
                {"B", new TestClass2()},
                {"C", new TestClass2()},
                {"D", new TestClass2()},
                {"E", new TestClass2()}
            };
        }

        #endregion

        #region Properties

        [IgnoreClone]
        private int PropertyOne { get; set; }

        [IgnoreClone]
        private string PropertyTwo { get; set; }

        [IgnoreClone]
        private decimal PropertyThree { get; set; }

        [IgnoreClone]
        private DateTime PropertyFour { get; set; }

        private int PropertyFive { get; set; }

        private decimal PropertySix { get; set; }

        private DateTime PropertySeven { get; set; }

        private string PropertyEight { get; set; }

        public string PropertyNine { get; set; }

        public TestClass2 PropertyTen { get; set; }

        [ShallowClone]
        public TestClass2 PropertyEleven { get; set; }

        [IgnoreClone]
        public TestClass2 PropertyTwelve { get; set; }

        public TestClass3[] PropertyThirteen { get; set; }

        [IgnoreClone]
        public TestClass3[] PropertyFourteen { get; set; }

        public TestClass2[] PropertyFifteen { get; set; }

        public List<TestClass2> PropertySixteen { get; set; }

        public Dictionary<string, TestClass2> PropertySeventeen { get; set; }

        #endregion

        public TestClass Clone()
        {
            return this.Copy();
        }

        public void Dispose()
        {
        }
    }
}