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

            double[,] result = { { 1, 2, 3 }, { 4, 10, 18 }, { 28, 80, 162 } };
            Matrix<double> R = new Matrix<double>(result);

            Assert.AreEqual(R, Matrix<double>.Cumprod(A));
        }

        [TestMethod]
        public void MatrixStringConstructor6x1()
        {
            string matrixString = "[7378.137; 0.0; 0.0; 0.0; 6.02088; 4.215866]";

            Matrix<double> A = new Matrix<double>(matrixString);

            double[,] result = { { 7378.137 }, { 0.0 }, { 0.0 }, { 0.0 }, { 6.02088 }, { 4.215866 } };
            Matrix<double> R = new Matrix<double>(result);

            Assert.AreEqual(R, A);
        }

        [TestMethod]
        public void MatrixStringConstructor3x3()
        {
            string matrixString = "[1, 2, 3; 4, 5, 6; 7, 8, 9]";

            Matrix<double> A = new Matrix<double>(matrixString);

            double[,] result = { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };
            Matrix<double> R = new Matrix<double>(result);

            Assert.AreEqual(R, A);
        }

        [TestMethod]
        public void MatrixToString()
        {
            string matrixString = "[1, 2, 3; 4, 5, 6; 7, 8, 9]";

            Matrix<double> A = new Matrix<double>(matrixString);

            Assert.AreEqual(matrixString, A.ToString());
        }
    }
}
