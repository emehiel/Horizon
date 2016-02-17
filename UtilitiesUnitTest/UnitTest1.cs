using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utilities;

namespace UtilitiesUnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            string arrayData = "[1, 2.3; 0, -2.1]";
            Matrix M = new Matrix(arrayData);

        }
    }
}
