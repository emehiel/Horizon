using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utilities;

namespace UtilitiesUnitTest
{
    [TestClass]
    public class ComplexUnitTests
    {
        [TestMethod]
        public void ComplexAddition()
        {
            Complex<double> complex1 = new Complex<double>(1, 1);
            Complex<double> complex2 = new Complex<double>(2, 2);
            Complex<double> complexSum = complex1 + complex2;

            Complex<double> complexAns = new Complex<double>(3, 3);

            Assert.AreEqual(complexAns, complexSum);
        }
        [TestMethod]
        public void ComplexSubtract()
        {
            Complex<double> complex1 = new Complex<double>(1, 1);
            Complex<double> complex2 = new Complex<double>(2, 2);
            Complex<double> complexSub = complex1 - complex2;

            Complex<double> complexAns = new Complex<double>(-1, -1);

            Assert.AreEqual(complexAns, complexSub);
        }
        [TestMethod]
        public void ComplexMultiply()
        {
            Complex<double> complex1 = new Complex<double>(1, 1);
            Complex<double> complex2 = new Complex<double>(2, 2);
            Complex<double> complexSum = complex1 * complex2;

            Complex<double> complexAns = new Complex<double>(0, 4);

            Assert.AreEqual(complexAns, complexSum);
        }
        [TestMethod]
        public void ComplexQuotient()
        {
            Complex<double> complex1 = new Complex<double>(1, 1);
            Complex<double> complex2 = new Complex<double>(2, 2);
            Complex<double> complexQuotient = complex1 / complex2;

            double complexAns = 0.5;

            Assert.AreEqual(complexAns, complexQuotient);
        }
        [TestMethod]
        public void ComplexInverse()
        {
            Complex<double> complex1 = new Complex<double>(1, 1);
            Complex<double> complexInv = -complex1;

            Complex<double> complexAns = new Complex<double>(-1, -1);

            Assert.AreEqual(complexAns, complexInv);
        }
        [TestMethod]
        public void ComplexGreaterThan()
        {
            Complex<double> complex1 = new Complex<double>(1, 1);
            Complex<double> complex2 = new Complex<double>(2, 2);
            bool complexAns = (complex1 > complex2);

            Assert.IsFalse(complexAns);
        }
        [TestMethod]
        public void ComplexLessThan()
        {
            Complex<double> complex1 = new Complex<double>(1, 1);
            Complex<double> complex2 = new Complex<double>(2, 2);
            bool complexAns = (complex1 < complex2);

            Assert.IsTrue(complexAns);
        }
        [TestMethod]
        public void ComplexGreaterThanEqual()
        {
            Complex<double> complex1 = new Complex<double>(1, 1);
            Complex<double> complex2 = new Complex<double>(1, 1);
            bool complexAns = (complex1 >= complex2);

            Assert.IsTrue(complexAns);
        }
        [TestMethod]
        public void ComplexLessThanEqual()
        {
            Complex<double> complex1 = new Complex<double>(1, 1);
            Complex<double> complex2 = new Complex<double>(1, 1);
            bool complexAns = (complex1 <= complex2);

            Assert.IsTrue(complexAns);
        }
        [TestMethod]
        public void ComplexAreEqual()
        {
            Complex<double> complex1 = new Complex<double>(1, 1);
            Complex<double> complex2 = new Complex<double>(1, 1);
            bool complexAns = (complex1 == complex2);

            Assert.IsTrue(complexAns);
        }
        [TestMethod]
        public void ComplexAreNotEqual()
        {
            Complex<double> complex1 = new Complex<double>(1, 1);
            Complex<double> complex2 = new Complex<double>(1, 1);
            bool complexAns = (complex1 != complex2);

            Assert.IsFalse(complexAns);
        }
        [TestMethod]
        public void ComplexConvertDoubleToReal()
        {
            double complex1 = 1;
            Complex<double> complex2 = (Complex<double>)complex1;
            Complex<double> complexAns = new Complex<double>(1, 0);
            Assert.AreEqual(complexAns, complex2);
        }
        [TestMethod]
        public void ComplexConvertRealToDouble()
        {
            Complex<double> complex1 = new Complex<double>(0, 1);
            double complex2 = (double)complex1;
            Assert.AreEqual(0, complex2);
        }
        [TestMethod]
        public void ComplexMatrixToComplex()
        {
            Matrix<double> A = new Matrix<double>(new double[,] { { 2 } });
            Complex<double> complex1 = (Complex<double>)A;
            Complex<double> complexAns = new Complex<double>(2);

            Assert.AreEqual(complexAns, complex1);
        }
        [TestMethod]
        public void ComplexConj()
        {
            Complex<double> complex1 = new Complex<double>(1, 2);
            Complex<double> complexConj = Complex<double>.Conj(complex1);

            Complex<double> complexAns = new Complex<double>(1, -2);

            Assert.AreEqual(complexAns, complexConj);
        }
        [TestMethod]
        public void ComplexAbs()
        {
            Complex<double> complex1 = new Complex<double>(3, 4);
            double complexAbs = Complex<double>.Abs(complex1);

            double complexAns = 5;

            Assert.AreEqual(complexAns, complexAbs);
        }
        [TestMethod]
        public void ComplexNorm()
        {
            Complex<double> complex1 = new Complex<double>(0, 0);
            double result = Complex<double>.Abs(complex1);

            double complexAns = 0;

            Assert.AreEqual(complexAns, result);
        }
        [TestMethod]
        public void ComplexAngle()
        {
            Complex<double> complex1 = new Complex<double>(3, 2);
            double result = Complex<double>.Angle(complex1);

            double complexAns = 0.5880;

            Assert.AreEqual(complexAns, result, 0.001);

        }
        [TestMethod]
        public void ComplexInv()
        {
            Complex<double> complex1 = new Complex<double>(-1, -3);
            Complex<double> result = Complex<double>.Inv(complex1);
            Complex<double> expected = new Complex<double>(-.1, .3);

            Assert.AreEqual(expected, result);
        }
        [TestMethod]
        public void ComplexExp()
        {
            Complex<double> complex1 = new Complex<double>(1, 4);
            Complex<double> result = Complex<double>.Exp(complex1);
            Complex<double> expected = new Complex<double>(-1.7768, -2.0572);

            Assert.AreEqual(expected.Re, result.Re, 0.0001);
            Assert.AreEqual(expected.Im, result.Im, 0.0001);
        }
        [TestMethod]
        public void ComplexMax()
        {
            Complex<double> complex1 = new Complex<double>(-1, -3);
            Complex<double> complex2 = new Complex<double>(1, 10);

            Complex<double> result = Complex<double>.Max(complex1, complex2);

            Complex<double> expected = new Complex<double>(1, 10);

            Assert.AreEqual(expected, result);
        }
        [TestMethod]
        public void ComplexMin()
        {
            Complex<double> complex1 = new Complex<double>(-1, -3);
            Complex<double> complex2 = new Complex<double>(1, 10);

            Complex<double> result = Complex<double>.Min(complex1, complex2);

            Complex<double> expected = new Complex<double>(-1, -3);

            Assert.AreEqual(expected, result);
        }
        [TestMethod]
        public void ComplexToString()
        {
            Complex<double> complex1 = new Complex<double>(0, 0);
            string result = complex1.ToString();

            //string expected = "0+0j";
            string expected = "0";
            Assert.AreEqual(expected, result);

        }
        [TestMethod]
        public void ComplexEquals()
        {
            Complex<double> complex1 = new Complex<double>(1, 0);
            bool result = complex1.Equals(complex1);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ComplexIsReal()
        {
            Complex<double> complex1 = new Complex<double>(1, 0);
            bool result = complex1.IsReal();

            Assert.IsTrue(result);
        }
        [TestMethod]
        public void ComplexIsImaginary()
        {
            Complex<double> complex1 = new Complex<double>(0, 1);
            bool result = complex1.IsImaginary();

            Assert.IsTrue(result);
        }
    }

}