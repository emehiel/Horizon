using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utilities;

namespace UtilitiesUnitTest
{
    [TestClass]
    public class HSFProfileUnitTest
    {
        [TestMethod]
        public void HSFProfileAddOperatorTest1()
        {
            HSFProfile<double> p1 = new HSFProfile<double>();
            HSFProfile<double> p2 = new HSFProfile<double>();

            p1[0] = 5;
            p1[10] = 15;

            p2[20] = 4;
            p2[30] = 12;

            HSFProfile<double> p3 = new HSFProfile<double>();

            p3 = p1 + p2;

            HSFProfile<double> result = new HSFProfile<double>();
            result[0] = 5;
            result[10] = 15;
            result[20] = 19;
            result[30] = 27;

            Assert.AreEqual(result, p3);
        }
        [TestMethod]
        public void HSFProfileAddOperatorTest2()
        {
            HSFProfile<double> p1 = new HSFProfile<double>();
            HSFProfile<double> p2 = new HSFProfile<double>();

            p1[0] = 5;
            p1[25] = 15;

            p2[20] = 4;
            p2[30] = 12;

            HSFProfile<double> p3 = new HSFProfile<double>();

            p3 = p1 + p2;

            HSFProfile<double> result = new HSFProfile<double>();
            result[0] = 5;
            result[20] = 9;
            result[25] = 19;
            result[30] = 27;

            Assert.AreEqual(result, p3);
        }

    }
}
