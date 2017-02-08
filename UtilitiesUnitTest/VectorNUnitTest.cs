
using System;
using NUnit.Framework;
using Utilities;
using System.Collections;

namespace UtilitiesUnitTest

{
    [TestFixture]
    public class VectorTest
    {
        [TestCaseSource(typeof(DotProductData), "TestCases")]
        public double DotTest(double[] a, double[] b)
        {
            return Vector.Dot(a, b);
        }

        [Test]
        public void CrossTest()
        {
            Vector a = new Vector(new double[] { 7378, 0, 0 });
            Vector b = new Vector(new double[] { -12525.2938790736, 1346.43186132083, -3517.5443929026 });
            Vector c = Vector.Cross(a, b);
            Vector d = new Vector(new double[] { 0, 25952442, 9933974 });

            Assert.AreEqual(d[1], c[1], 1);
            Assert.AreEqual(d[2], c[2], 1);
            Assert.AreEqual(d[3], c[3], 1);
        }
        [Test]
        public void NormTest()
        {
            Vector a = new Vector(new double[] { -12525.2938790736, 1346.43186132083, -3517.5443929026 });
            double b = Vector.Norm(a);
            double c = 13079;
            Assert.AreEqual(c, b, 1);
        }
        [Test]
        public void MatrixVectorMultiplicationTest()
        {
            Vector a = new Vector(new double[] { 1, 2, 3 });
            Matrix<double> B = new Matrix<double>(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } });
            Vector result = a * B;
            Vector expected = new Vector(new double[] { 30, 36, 42 });
            Assert.AreEqual(expected, result);

            B = new Matrix<double>(new double[,] { { 1 }, { 2 }, { 3 } });
            result = a * B;
            expected = new Vector(new double[] { 14 });
            Assert.AreEqual(expected, result);
        }
    }
    public class DotProductData
    {
        public static IEnumerable TestCases
        {
            get
            {
                yield return new TestCaseData(new double[] { 7378, 0, 0 }, new double[] { -12525, 1346, -3517 }).Returns(-92409450).SetName("Large Negative Numbers");
                yield return new TestCaseData(new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }).Returns(285).SetName("Large Vector");
                yield return new TestCaseData(new double[] { 0 }, new double[] { 0 }).Returns(0).SetName("Zero Test");
            }
        }
    }
    public class CrossProductData
    {
        public static IEnumerable TestCases
        {
            get
            {
                yield return new TestCaseData(new double[] { 7378, 0, 0 }, new double[] { -12525, 1346, -3517 }).Returns(-92409450).SetName("Large Negative Numbers");
                yield return new TestCaseData(new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }).SetName("Large Vector");
                yield return new TestCaseData(new double[] { 0 }, new double[] { 0 }).Returns(0).SetName("Zero Test");
            }
        }
    }
}