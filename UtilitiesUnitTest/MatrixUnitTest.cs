using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utilities;

namespace UtilitiesUnitTest
{
    [TestClass]
    public class MatrixUnitTest
    {
        [TestMethod]
        public void TestDot()
        {
            Matrix<double> A = new Matrix<double>(3, 1, 2);
            Matrix<double> B = new Matrix<double>(3, 1, 4);

            double answer = 24;


            Assert.AreEqual(Matrix<double>.(A, B), answer);
        }
    }
}
