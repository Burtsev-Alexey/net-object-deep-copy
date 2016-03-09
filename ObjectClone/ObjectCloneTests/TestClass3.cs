using System;
using ObjectClone.Attributes;

namespace ObjectCloneTests
{
    [IgnoreClone]
    public class TestClass3 : IDisposable
    {
        #region Ctors

        public TestClass3()
        {
            this.PropertyOne = "TestClass3.PropertyOne";
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