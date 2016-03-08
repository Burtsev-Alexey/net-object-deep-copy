using System;
using ObjectClone.Attributes;

namespace ObjectCloneTests
{
    [ShallowClone]
    public class TestClass4 : IDisposable
    {
        #region Ctors

        public TestClass4()
        {
            this.PropertyOne = "TestClass4.PropertyOne";
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