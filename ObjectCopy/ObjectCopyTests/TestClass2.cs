using System;

namespace ObjectCloneTests
{
    public class TestClass2 : IDisposable
    {
        #region Ctors

        public TestClass2()
        {
            this.PropertyOne = "TestClass2.PropertyOne";
        }

        #endregion

        #region Properties

        public string PropertyOne { get; set; }

        #endregion

        public void Dispose()
        {
        }
    }
}