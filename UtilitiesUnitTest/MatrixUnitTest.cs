using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utilities;

namespace UtilitiesUnitTest
{
    [TestClass]
    public class MatrixUnitTest
    {
        [TestMethod]
        public void TestCumProd1()
        {
            double[,] elements = { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };
            Matrix<double> A = new Matrix<double>(elements);

            double[,] result = { { 1, 2, 4 }, { 4, 10, 18 }, { 28, 80, 162 } };
            Matrix<double> R = new Matrix<double>(result);

            Assert.AreEqual(R, Matrix<double>.Cumprod(A));
        }
    }
}
